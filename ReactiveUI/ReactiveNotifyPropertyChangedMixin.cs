﻿using System;
using System.Collections.Generic;
using System.Disposables;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ReactiveUI
{
    public static class ReactiveNotifyPropertyChangedMixin
    {
        /// <summary>
        /// ObservableForProperty returns an Observable representing the
        /// property change notifications for a specific property on a
        /// ReactiveObject. This method (unlike other Observables that return
        /// IObservedChange) guarantees that the Value property of
        /// the IObservedChange is set.
        /// </summary>
        /// <param name="property">An Expression representing the property (i.e.
        /// 'x => x.SomeProperty'</param>
        /// <param name="beforeChange">If True, the Observable will notify
        /// immediately before a property is going to change.</param>
        /// <returns>An Observable representing the property change
        /// notifications for the given property.</returns>
        public static IObservable<IObservedChange<TSender, TValue>> OldObservableForProperty<TSender, TValue>(
                this TSender This, 
                Expression<Func<TSender, TValue>> property, 
                bool beforeChange = false)
            where TSender : IReactiveNotifyPropertyChanged
        {
            Contract.Requires(This != null);
            Contract.Requires(property != null);

            string prop_name = RxApp.simpleExpressionToPropertyName(property);
            var prop_info = RxApp.getPropertyInfoForProperty<TSender>(prop_name);

            This.Log().InfoFormat("Registering change notification for {0:X} on {1}", This.GetHashCode(), prop_name);

            if (beforeChange) {
                return This.Changing
                    .Where(x => x.PropertyName == prop_name)
                    .Select(x => (IObservedChange<TSender, TValue>) new ObservedChange<TSender, TValue>() { 
                        Sender = This, PropertyName = prop_name, Value = (TValue)prop_info.GetValue(This, null)
                    });
            }

            return This.Changed
                .Where(x => x.PropertyName == prop_name)
                .Select(x => (IObservedChange<TSender, TValue>) new ObservedChange<TSender, TValue>() { 
                    Sender = This, PropertyName = prop_name, Value = (TValue)prop_info.GetValue(This, null)
                });
        }

        public static IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TSender, TValue>(
                this TSender This,
                Expression<Func<TSender, TValue>> property,
                bool beforeChange = false)
            where TSender : IReactiveNotifyPropertyChanged
        {
            var propertyNames = new LinkedList<string>(RxApp.expressionToPropertyNames(property));
            var subscriptions = new LinkedList<IDisposable>(propertyNames.Select(x => (IDisposable) null));
            var ret = new Subject<IObservedChange<TSender, TValue>>();

            subscribeToExpressionChain(
                This, 
                buildPropPathFromNodePtr(propertyNames.First),
                This, 
                propertyNames.First, 
                subscriptions.First, 
                beforeChange, 
                ret);

            return Observable.Create<IObservedChange<TSender, TValue>>(x => {
                var disp = ret.Subscribe(x.OnNext, x.OnError, x.OnCompleted);
                return () => {
                    subscriptions.Run(y => y.Dispose());
                    disp.Dispose();
                };
            });
        }

        static void subscribeToExpressionChain<TSender, TValue>(
                TSender origSource,
                string origPath,
                object source,
                LinkedListNode<string> propertyNames, 
                LinkedListNode<IDisposable> subscriptions, 
                bool beforeChange,
                Subject<IObservedChange<TSender, TValue>> subject
            )
        {
            var current = propertyNames;
            var currentSub = subscriptions;
            object currentObj = source;
            PropertyInfo pi = null;
            ObservedChange<TSender, TValue> obsCh;

            while(current.Next != null) {
                pi = RxApp.getPropertyInfoForProperty(currentObj.GetType(), current.Value);
                if (pi == null) {
                    subscriptions.List.Where(x => x != null).Run(x => x.Dispose());
                    throw new ArgumentException(String.Format("Property '{0}' does not exist in expression", current.Value));
                }

                var notifyObj = currentObj as IReactiveNotifyPropertyChanged;
                if (notifyObj != null) {
                    TValue prevVal = default(TValue);
                    var capture = new {current, currentObj, pi, currentSub};
                    var toDispose = new IDisposable[2];

                    var valGetter = new ObservedChange<object, TValue>() {
                        Sender = capture.currentObj,
                        PropertyName = buildPropPathFromNodePtr(capture.current),
                        Value = default(TValue),
                    };

                    toDispose[0] = notifyObj.Changing.Where(x => x.PropertyName == capture.current.Value).Subscribe(x => {
                        prevVal = valGetter.GetValue();
                    });

                    toDispose[1] = notifyObj.Changed.Where(x => x.PropertyName == capture.current.Value).Subscribe(x => {
                        subscribeToExpressionChain(origSource, origPath, capture.pi.GetValue(capture.currentObj, null), capture.current.Next, capture.currentSub.Next, beforeChange, subject);

                        var newVal = valGetter.GetValue();
                        if (EqualityComparer<TValue>.Default.Equals(prevVal, newVal)) {
                            return;
                        }

                        obsCh = new ObservedChange<TSender, TValue>() {
                            Sender = origSource,
                            PropertyName = origPath,
                            Value = default(TValue),
                        };
                        obsCh.Value = obsCh.GetValue();
                        subject.OnNext(obsCh);
                    });

                    currentSub.Value = Disposable.Create(() => { toDispose[0].Dispose(); toDispose[1].Dispose(); });
                }

                current = current.Next;
                currentSub = currentSub.Next;
                currentObj = pi.GetValue(currentObj, null);
            }

            var finalNotify = (IReactiveNotifyPropertyChanged)currentObj;
            if (currentSub.Value != null) {
                currentSub.Value.Dispose();
            }

            if (finalNotify == null) {
                return;
            }

            var propName = current.Value;
            pi = RxApp.getPropertyInfoForProperty(currentObj.GetType(), current.Value);

            currentSub.Value = (beforeChange ? finalNotify.Changing : finalNotify.Changed).Subscribe(x => {
                obsCh = new ObservedChange<TSender, TValue>() {
                    Sender = origSource,
                    PropertyName = origPath,
                    Value = (TValue)pi.GetValue(currentObj, null),
                };

                subject.OnNext(obsCh);
            });
        }

        static string buildPropPathFromNodePtr(LinkedListNode<string> node)
        {
            var ret = new StringBuilder();
            var current = node;

            while(current.Next != null) {
                ret.Append(current.Value);
                ret.Append('.');
                current = current.Next;
            }

            ret.Append(current.Value);
            return ret.ToString();
        }


        /// <summary>
        /// ObservableForProperty returns an Observable representing the
        /// property change notifications for a specific property on a
        /// ReactiveObject, running the IObservedChange through a Selector
        /// function.
        /// </summary>
        /// <param name="property">An Expression representing the property (i.e.
        /// 'x => x.SomeProperty'</param>
        /// <param name="selector">A Select function that will be run on each
        /// item.</param>
        /// <param name="beforeChange">If True, the Observable will notify
        /// immediately before a property is going to change.</param>
        /// <returns>An Observable representing the property change
        /// notifications for the given property.</returns>
        public static IObservable<TRet> ObservableForProperty<TSender, TValue, TRet>(
                this TSender This, 
                Expression<Func<TSender, TValue>> property, 
                Func<TValue, TRet> selector, 
                bool beforeChange = false)
            where TSender : IReactiveNotifyPropertyChanged
        {           
            Contract.Requires(selector != null);
            return This.ObservableForProperty(property, beforeChange).Select(x => selector(x.Value));
        }
    }
}

// vim: tw=120 ts=4 sw=4 et :
