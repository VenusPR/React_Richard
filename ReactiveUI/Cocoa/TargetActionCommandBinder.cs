using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;

#if UIKIT
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
#else
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
#endif

namespace ReactiveUI.Cocoa
{
    /// <summary>
    /// TargetActionCommandBinder is an implementation of command binding that
    /// understands Cocoa's Target / Action Framework. Many controls in Cocoa 
    /// that are effectively command sources (i.e. Buttons, Menus, etc), 
    /// participate in this framework.
    /// </summary>
    public class TargetActionCommandBinder : ICreatesCommandBinding
    {
        readonly Type[] validTypes;
        public TargetActionCommandBinder() 
        {
#if UIKIT
            validTypes = new[] {
                typeof(UIControl),
             };
#else
            validTypes = new[] {
                typeof(NSControl),
                typeof(NSCell),
                typeof(NSMenu),
                typeof(NSMenuItem),
                typeof(NSToolbarItem),
            };
#endif
        }

        public int GetAffinityForObject(Type type, bool hasEventTarget)
        {
            if (!validTypes.Any(x => x.IsAssignableFrom(type))) return 0;
            return !hasEventTarget ? 4 : 0;
        }

        public IDisposable BindCommandToObject(ICommand command, object target, IObservable<object> commandParameter)
        {
            commandParameter = commandParameter ?? Observable.Return(target);

            object latestParam = null;
            var ctlDelegate = new ControlDelegate(x => {
                if (command.CanExecute(latestParam))
                    command.Execute(latestParam);
            });

            var sel = new Selector("theAction:");
            // TODO how does this work? Is there an Action property?
            Reflection.GetValueSetterOrThrow(target.GetType().GetRuntimeProperty("Action"))(target, sel, null);

            var targetSetter = Reflection.GetValueSetterOrThrow(target.GetType().GetRuntimeProperty("Target"));
            targetSetter(target, ctlDelegate, null);
            var actionDisp = Disposable.Create(() => targetSetter(target, null, null));

            var enabledSetter = Reflection.GetValueSetterForProperty(target.GetType().GetRuntimeProperty("Enabled"));
            if(enabledSetter == null) return actionDisp;

            // initial enabled state
            enabledSetter(target, command.CanExecute(latestParam), null);

            var compDisp = new CompositeDisposable(
                actionDisp,
                commandParameter.Subscribe(x => latestParam = x),
                Observable.FromEventPattern<EventHandler, EventArgs>(x => command.CanExecuteChanged += x, x => command.CanExecuteChanged -= x)
                    .Select(_ => command.CanExecute(latestParam))
                    .Subscribe(x => { enabledSetter(target, x, null); }));

            return compDisp;
        }

        class ControlDelegate : NSObject
        {
            readonly Action<NSObject> block;
            public ControlDelegate(Action<NSObject> block)
            {
                this.block = block;
            }

            [Export("theAction:")]
            public void TheAction(NSObject sender)
            {
                block(sender);
            }
        }

        public IDisposable BindCommandToObject<TEventArgs>(ICommand command, object target, IObservable<object> commandParameter, string eventName) 
            where TEventArgs : EventArgs
        {
            throw new NotImplementedException();
        }
    }
}
