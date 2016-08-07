namespace KeyboardSwitcher.KeyBinding
{
    partial class KeyBindingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtModuleId = new System.Windows.Forms.TextBox();
            this.txtParam = new System.Windows.Forms.TextBox();
            this.txtBindingKey = new System.Windows.Forms.TextBox();
            this.cmbBindingType = new System.Windows.Forms.ComboBox();
            this.cbExclusive = new System.Windows.Forms.CheckBox();
            this.cbEnableHandled = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tmrHotKeyUpdate = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Модуль";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Параметры";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Клавиши";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Метод";
            // 
            // txtModuleId
            // 
            this.txtModuleId.Location = new System.Drawing.Point(127, 10);
            this.txtModuleId.Name = "txtModuleId";
            this.txtModuleId.Size = new System.Drawing.Size(75, 20);
            this.txtModuleId.TabIndex = 4;
            // 
            // txtParam
            // 
            this.txtParam.Location = new System.Drawing.Point(127, 36);
            this.txtParam.Name = "txtParam";
            this.txtParam.Size = new System.Drawing.Size(75, 20);
            this.txtParam.TabIndex = 5;
            // 
            // txtBindingKey
            // 
            this.txtBindingKey.Location = new System.Drawing.Point(127, 71);
            this.txtBindingKey.Name = "txtBindingKey";
            this.txtBindingKey.Size = new System.Drawing.Size(222, 20);
            this.txtBindingKey.TabIndex = 6;
            this.txtBindingKey.Enter += new System.EventHandler(this.txtBindingKey_Enter);
            // 
            // cmbBindingType
            // 
            this.cmbBindingType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBindingType.FormattingEnabled = true;
            this.cmbBindingType.Location = new System.Drawing.Point(127, 97);
            this.cmbBindingType.Name = "cmbBindingType";
            this.cmbBindingType.Size = new System.Drawing.Size(222, 21);
            this.cmbBindingType.TabIndex = 7;
            // 
            // cbExclusive
            // 
            this.cbExclusive.AutoSize = true;
            this.cbExclusive.Location = new System.Drawing.Point(127, 124);
            this.cbExclusive.Name = "cbExclusive";
            this.cbExclusive.Size = new System.Drawing.Size(103, 17);
            this.cbExclusive.TabIndex = 8;
            this.cbExclusive.Text = "Эксклюзивный";
            this.cbExclusive.UseVisualStyleBackColor = true;
            // 
            // cbEnableHandled
            // 
            this.cbEnableHandled.AutoSize = true;
            this.cbEnableHandled.Location = new System.Drawing.Point(127, 147);
            this.cbEnableHandled.Name = "cbEnableHandled";
            this.cbEnableHandled.Size = new System.Drawing.Size(112, 17);
            this.cbEnableHandled.TabIndex = 9;
            this.cbEnableHandled.Text = "Запретить далее";
            this.cbEnableHandled.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(184, 184);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "Ок";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(274, 184);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tmrHotKeyUpdate
            // 
            this.tmrHotKeyUpdate.Tick += new System.EventHandler(this.tmrHotKeyUpdate_Tick);
            // 
            // KeyBindingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(375, 219);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cbEnableHandled);
            this.Controls.Add(this.cbExclusive);
            this.Controls.Add(this.cmbBindingType);
            this.Controls.Add(this.txtBindingKey);
            this.Controls.Add(this.txtParam);
            this.Controls.Add(this.txtModuleId);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "KeyBindingForm";
            this.Text = "KeyBindingForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtModuleId;
        private System.Windows.Forms.TextBox txtParam;
        private System.Windows.Forms.TextBox txtBindingKey;
        private System.Windows.Forms.ComboBox cmbBindingType;
        private System.Windows.Forms.CheckBox cbExclusive;
        private System.Windows.Forms.CheckBox cbEnableHandled;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Timer tmrHotKeyUpdate;
    }
}