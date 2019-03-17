using System;

namespace ark_RebarDrawing
{
    public partial class arkRebarDrawingForm : Tekla.Structures.Dialog.PluginFormBase
    {
        public arkRebarDrawingForm()
        {
            InitializeComponent();
        }

        private void OkApplyModifyGetOnOffCancel_OkClicked(object sender, EventArgs e)
        {
            this.Apply();
            this.Close();
        }

        private void OkApplyModifyGetOnOffCancel_ApplyClicked(object sender, EventArgs e)
        {
            this.Apply();
        }

        private void OkApplyModifyGetOnOffCancel_CancelClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OkApplyModifyGetOnOffCancel_GetClicked(object sender, EventArgs e)
        {
            this.Get();
        }

        private void OkApplyModifyGetOnOffCancel_ModifyClicked(object sender, EventArgs e)
        {
            this.Modify();
        }

        private void OkApplyModifyGetOnOffCancel_OnOffClicked(object sender, EventArgs e)
        {
            this.ToggleSelection();
        }
    }
}
