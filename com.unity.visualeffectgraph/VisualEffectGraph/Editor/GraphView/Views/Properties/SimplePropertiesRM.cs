using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements;
using UnityEditor.VFX;
using UnityEditor.VFX.UIElements;
using Object = UnityEngine.Object;
using Type = System.Type;
using EnumField = UnityEditor.VFX.UIElements.VFXEnumField;
using VFXVector2Field = UnityEditor.VFX.UIElements.VFXVector2Field;
using VFXVector4Field = UnityEditor.VFX.UIElements.VFXVector4Field;
using FloatField = UnityEditor.VFX.UIElements.VFXFloatField;

namespace UnityEditor.VFX
{
    interface IStringProvider
    {
        string[] GetAvailableString();
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class StringProviderAttribute : PropertyAttribute
    {
        public StringProviderAttribute(Type providerType)
        {
            if (!typeof(IStringProvider).IsAssignableFrom(providerType))
                throw new InvalidCastException("StringProviderAttribute excepts a type which implements interface IStringProvider : " + providerType);
            this.providerType = providerType;
        }

        public Type providerType { get; private set; }
    }

    interface IPushButtonBehavior
    {
        void OnClicked(string currentValue);
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class PushButtonAttribute : PropertyAttribute
    {
        public PushButtonAttribute(Type pushButtonProvider, string buttonName)
        {
            if (!typeof(IPushButtonBehavior).IsAssignableFrom(pushButtonProvider))
                throw new InvalidCastException("PushButtonAttribute excepts a type which implements interface IPushButtonBehavior : " + pushButtonProvider);
            this.pushButtonProvider = pushButtonProvider;
            this.buttonName = buttonName;
        }

        public Type pushButtonProvider { get; private set; }
        public string buttonName { get; private set; }
    }
}

namespace UnityEditor.VFX.UI
{
    class UintPropertyRM : SimpleUIPropertyRM<uint, long>
    {
        public UintPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override float GetPreferredControlWidth()
        {
            return 60;
        }

        public override INotifyValueChanged<long> CreateField()
        {
            Vector2 range = VFXPropertyAttribute.FindRange(VFXPropertyAttribute.Create(m_Provider.attributes));
            if (range == Vector2.zero || range.y == Mathf.Infinity || (uint)range.x >= (uint)range.y)
            {
                var field = new VFXLabeledField<IntegerField, long>(m_Label);
                return field;
            }
            else
            {
                range.x = Mathf.Max(0, Mathf.Round(range.x));
                range.y = Mathf.Max(range.x + 1, Mathf.Round(range.y));

                var field = new VFXLabeledField<VFXIntSliderField, long>(m_Label);
                field.control.range = range;
                return field;
            }
        }

        protected override bool HasFocus()
        {
            if (field is VFXLabeledField<IntegerField, long>)
                return (field as VFXLabeledField<IntegerField, long>).control.hasFocus;
            return (field as VFXLabeledField<VFXIntSliderField, long>).control.hasFocus;
        }

        public override object FilterValue(object value)
        {
            Vector2 range = VFXPropertyAttribute.FindRange(m_Provider.attributes);

            if (range != Vector2.zero && (range.y == Mathf.Infinity || (uint)range.x < (uint)range.y))
            {
                uint val = (uint)value;

                if (range.x > val)
                {
                    val = (uint)range.x;
                }
                if (range.y < val)
                {
                    val = (uint)range.y;
                }

                value = val;
            }

            return value;
        }
    }

    class IntPropertyRM : SimpleUIPropertyRM<int, long>
    {
        public IntPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override INotifyValueChanged<long> CreateField()
        {
            Vector2 range = VFXPropertyAttribute.FindRange(m_Provider.attributes);
            if (range == Vector2.zero || range.y == Mathf.Infinity || (int)range.x >= (int)range.y)
            {
                var field = new VFXLabeledField<IntegerField, long>(m_Label);
                return field;
            }
            else
            {
                range.x = Mathf.Round(range.x);
                range.y = Mathf.Max(range.x + 1, Mathf.Round(range.y));

                var field = new VFXLabeledField<VFXIntSliderField, long>(m_Label);
                field.control.range = range;
                return field;
            }
        }

        protected override bool HasFocus()
        {
            if (field is VFXLabeledField<IntegerField, long>)
                return (field as VFXLabeledField<IntegerField, long>).control.hasFocus;
            return (field as VFXLabeledField<VFXIntSliderField, long>).control.hasFocus;
        }

        public override object FilterValue(object value)
        {
            Vector2 range = VFXPropertyAttribute.FindRange(m_Provider.attributes);

            if (range != Vector2.zero && (range.y == Mathf.Infinity || (int)range.x < (int)range.y))
            {
                int val = (int)value;

                if (range.x > val)
                {
                    val = (int)range.x;
                }
                if (range.y < val)
                {
                    val = (int)range.y;
                }

                value = val;
            }

            return value;
        }

        public override float GetPreferredControlWidth()
        {
            return 60;
        }
    }
    class EnumPropertyRM : SimplePropertyRM<int>
    {
        public EnumPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override float GetPreferredControlWidth()
        {
            return 120;
        }

        public override ValueControl<int> CreateField()
        {
            return new EnumField(m_Label, m_Provider.portType);
        }
    }

    class FloatPropertyRM : SimpleUIPropertyRM<float, float>
    {
        public FloatPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        VFXDoubleSliderField m_SliderField;

        public override INotifyValueChanged<float> CreateField()
        {
            Vector2 range = VFXPropertyAttribute.FindRange(m_Provider.attributes);

            if (range == Vector2.zero || range.y == Mathf.Infinity)
            {
                var field = new VFXLabeledField<FloatField, float>(m_Label);
                return field;
            }
            else
            {
                var field = new VFXLabeledField<VFXDoubleSliderField, float>(m_Label);
                field.control.range = range;
                m_SliderField = field.control;
                return field;
            }
        }

        public override bool IsCompatible(IPropertyRMProvider provider)
        {
            if (!base.IsCompatible(provider)) return false;

            Vector2 range = VFXPropertyAttribute.FindRange(m_Provider.attributes);


            return (range == Vector2.zero || range.y == Mathf.Infinity) == (m_SliderField == null);
        }

        public override void UpdateGUI()
        {
            if (m_SliderField != null)
            {
                Vector2 range = VFXPropertyAttribute.FindRange(m_Provider.attributes);

                m_SliderField.range = range;
            }
            base.UpdateGUI();
        }

        protected override bool HasFocus()
        {
            if (field is VFXLabeledField<FloatField, float>)
                return (field as VFXLabeledField<FloatField, float>).control.hasFocus;
            return (field as VFXLabeledField<VFXDoubleSliderField, float>).control.hasFocus;
        }

        public override object FilterValue(object value)
        {
            Vector2 range = VFXPropertyAttribute.FindRange(m_Provider.attributes);

            if (range != Vector2.zero && range.x < range.y)
            {
                float val = (float)value;

                if (range.x > val)
                {
                    val = range.x;
                }
                if (range.y < val)
                {
                    val = range.y;
                }

                value = val;
            }

            return value;
        }

        public override float GetPreferredControlWidth()
        {
            return 100;
        }
    }

    class Vector4PropertyRM : SimpleUIPropertyRM<Vector4, Vector4>
    {
        public Vector4PropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override INotifyValueChanged<Vector4> CreateField()
        {
            var field = new VFXLabeledField<VFXVector4Field, Vector4>(m_Label);

            return field;
        }

        public override float GetPreferredControlWidth()
        {
            return 180;
        }
    }

    class Matrix4x4PropertyRM : SimpleUIPropertyRM<Matrix4x4, Matrix4x4>
    {
        public Matrix4x4PropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override INotifyValueChanged<Matrix4x4> CreateField()
        {
            var field = new VFXLabeledField<VFXMatrix4x4Field, Matrix4x4>(m_Label);

            return field;
        }

        public override float GetPreferredControlWidth()
        {
            return 260;
        }
    }

    class Vector2PropertyRM : SimpleUIPropertyRM<Vector2, Vector2>
    {
        public Vector2PropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override INotifyValueChanged<Vector2> CreateField()
        {
            var field = new VFXLabeledField<VFXVector2Field, Vector2>(m_Label);

            return field;
        }

        public override float GetPreferredControlWidth()
        {
            return 100;
        }
    }

    class FlipBookPropertyRM : SimpleUIPropertyRM<FlipBook, FlipBook>
    {
        public FlipBookPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override INotifyValueChanged<FlipBook> CreateField()
        {
            var field = new VFXLabeledField<VFXFlipBookField, FlipBook>(m_Label);

            return field;
        }

        public override float GetPreferredControlWidth()
        {
            return 100;
        }
    }

    class StringPropertyRM : SimplePropertyRM<string>
    {
        public StringPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
        }

        public override float GetPreferredControlWidth()
        {
            return 140;
        }

        public static Func<string[]> FindStringProvider(object[] customAttributes)
        {
            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    if (attribute is StringProviderAttribute)
                    {
                        var instance = Activator.CreateInstance((attribute as StringProviderAttribute).providerType);
                        var stringProvider = instance as IStringProvider;
                        return () => stringProvider.GetAvailableString();
                    }
                }
            }
            return null;
        }

        public struct StringPushButtonInfo
        {
            public Action<string> action;
            public string buttonName;
        }

        public static StringPushButtonInfo FindPushButtonBehavior(object[] customAttributes)
        {
            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    if (attribute is PushButtonAttribute)
                    {
                        var instance = Activator.CreateInstance((attribute as PushButtonAttribute).pushButtonProvider);
                        var pushButtonBehavior = instance as IPushButtonBehavior;
                        return new StringPushButtonInfo() {action = (a) => pushButtonBehavior.OnClicked(a), buttonName = (attribute as PushButtonAttribute).buttonName};
                    }
                }
            }
            return new StringPushButtonInfo();
        }

        public override ValueControl<string> CreateField()
        {
            var stringProvider = FindStringProvider(m_Provider.customAttributes);
            var pushButtonProvider = FindPushButtonBehavior(m_Provider.customAttributes);
            if (stringProvider != null)
            {
                return new VFXStringFieldProvider(m_Label, stringProvider);
            }
            else if (pushButtonProvider.action != null)
            {
                return new VFXStringFieldPushButton(m_Label, pushButtonProvider.action, pushButtonProvider.buttonName);
            }
            else
            {
                return new VFXStringField(m_Label);
            }
        }

        public override bool IsCompatible(IPropertyRMProvider provider)
        {
            if (!base.IsCompatible(provider)) return false;

            var stringProvider = FindStringProvider(m_Provider.customAttributes);
            var pushButtonInfo = FindPushButtonBehavior(m_Provider.customAttributes);

            if (stringProvider != null)
            {
                return m_Field is VFXStringFieldProvider && (m_Field as VFXStringFieldProvider).stringProvider == stringProvider;
            }
            else if (pushButtonInfo.action != null)
            {
                return m_Field is VFXStringFieldPushButton && (m_Field as VFXStringFieldPushButton).pushButtonProvider == pushButtonInfo.action;
            }

            return !(m_Field is VFXStringFieldProvider) && !(m_Field is VFXStringFieldPushButton);
        }
    }
}
