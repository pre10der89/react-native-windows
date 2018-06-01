// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ReactNative.UIManager;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

namespace ReactNative.Accessibility
{
    /// <summary>
    /// Automation peer that allows to dynamically change behavior.
    /// </summary>
    /// <typeparam name="T">Type of owner of the automation peer.</typeparam>
    public sealed class DynamicAutomationPeer<T> : FrameworkElementAutomationPeer, IInvokeProvider
        where T : FrameworkElement, IAccessible
    {
        /// <summary>
        /// Hides base UIElement Owner to provide stronger-typed T Owner.
        /// </summary>
        private new T Owner => (T)base.Owner;

        /// <inheritdoc />
        public DynamicAutomationPeer(T owner)
            : base(owner)
        {
        }

        /// <inheritdoc />
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            if (Owner.AccessibilityTraits?.Contains(AccessibilityTrait.ListItem) == true)
            {
                return AutomationControlType.ListItem;
            }
            if (Owner.AccessibilityTraits?.Contains(AccessibilityTrait.Button) == true)
            {
                return AutomationControlType.Button;
            }

            // We expose a view that hides all children but makes itself visible to screen reader
            // with an (expected) accessible name as Text control type instead of Group to avoid
            // "group" suffix screen reader appends to the name.
            // Another argument for this is that it's not ideal to tell user that something without children
            // is a "group".
            var isLabelSet = !string.IsNullOrEmpty(AccessibilityHelper.GetAccessibilityLabel(Owner));
            var i4a = AccessibilityHelper.GetImportantForAccessibility(Owner);
            if ( i4a == ImportantForAccessibility.Yes
                || (i4a == ImportantForAccessibility.Auto && isLabelSet))
            {
                return AutomationControlType.Text;
            }

            return AutomationControlType.Group;
        }

        /// <inheritdoc />
        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Invoke
                && Owner.AccessibilityTraits?.Contains(AccessibilityTrait.Button) == true)
            {
                return this;
            }
            if (Owner.AccessibilityTraits?.Contains(AccessibilityTrait.ListItem) == true)
            {
                return base.GetPatternCore(patternInterface);
            }
            return null;
        }

        /// <inheritdoc />
        public void Invoke()
        {
            Owner.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(new AccessibilityTapEvent(Owner.GetTag()));
        }
    }
}
