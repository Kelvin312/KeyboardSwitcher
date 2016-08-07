using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PostSwitcher;

namespace KeyboardSwitcher.KeyBinding
{
    public partial class KeyBindingForm : Form
    {
        private readonly BindingManager _bindingManager;
        private List<Keys> _bindingKeys;
        public BindingItem ResultItem { get; private set; }
        public KeyBindingForm(BindingManager bindingManager)
        {
            InitializeComponent();
            _bindingManager = bindingManager;
            _bindingKeys = new List<Keys>();

            cmbBindingType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBindingType.ValueMember = "value";
            cmbBindingType.DisplayMember = "Description";
            cmbBindingType.DataSource = Enum.GetValues(typeof(BindingType))
                .Cast<Enum>()
                .Select(value => new
                {
                    (Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()),
                        typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description,
                    value
                })
                .Where(value => value.Description != null)
                .ToList();
        }


        public void CreateDialog(BindingItem item)
        {
            _bindingKeys.Clear();
            _bindingKeys.AddRange(item.KeyList);
            txtModuleId.Text = item.ModuleId.ToString();
            txtParam.Text = item.Param.ToString();
            UpdateKeyString();
            cmbBindingType.SelectedValue = item.KeyType & BindingType.Mask;
            cbExclusive.Checked = item.KeyType.HasFlag(BindingType.IsOtherNotPress);
            cbEnableHandled.Checked = item.KeyType.HasFlag(BindingType.IsEnableHandled);
        }

        private void UpdateKeyString()
        {
            txtBindingKey.Text = string.Join(" + ", _bindingKeys.Select(x => x.ToString()));
        }

        private void GenerateItem()
        {
            BindingType type = (BindingType)cmbBindingType.SelectedValue;
            if(cbExclusive.Checked) type |= BindingType.IsOtherNotPress;
            if (cbEnableHandled.Checked) type |= BindingType.IsEnableHandled;
            int moduleId = int.Parse(txtModuleId.Text);
            int param = int.Parse(txtParam.Text);
            ResultItem = new BindingItem(_bindingKeys,type,moduleId,param);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            GenerateItem();
            this.DialogResult = DialogResult.OK;
            //Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            //Close();
        }

        private void tmrHotKeyUpdate_Tick(object sender, EventArgs e)
        {
            _bindingKeys.Clear();
            _bindingKeys.AddRange(_bindingManager.LastPressKeys);
            if(!txtBindingKey.Focused && _bindingManager.IsAnyNotPressed) tmrHotKeyUpdate.Stop();
            UpdateKeyString();
        }

        private void txtBindingKey_Enter(object sender, EventArgs e)
        {
            if (!tmrHotKeyUpdate.Enabled)
            {
                _bindingManager.LastPressKeys.Clear();
                tmrHotKeyUpdate.Start();
            }
        }
    }
}

//        EnumWithName<T>
