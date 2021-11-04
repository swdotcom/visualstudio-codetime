using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeTime
{
    public class Notification
    {
        public static DialogResult Show(string title, string promptText)
        {
            return Show(title, promptText, null);
        }

        public static DialogResult Show(string title, string promptText,
                                        InputBoxValidation validation)
        {
            Form form = new Form();
            Label label = new Label();
            Button buttonOk = new Button();

            form.Text = title;
            label.Text = promptText;

            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;

            label.SetBounds(9, 20, 372, 13);
            buttonOk.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, buttonOk });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            DialogResult dialogResult = form.ShowDialog();
            return dialogResult;
        }

        public static void InitiateSignupFlow()
        {
            string msg = "Connecting Slack requires a registered account. Sign up or log in to continue.";
            DialogResult res = MessageBox.Show(msg, "Registration", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            if (res == DialogResult.OK)
            {
                ShowSignUpDialog();
            }
        }

        [STAThread]
        private static void ShowSignUpDialog()
        {
            // show the sign up flow
            SwitchAccountDialog dialog = new SwitchAccountDialog();
            dialog.ShowDialog();

            string authType = dialog.getSelection();
            if (!string.IsNullOrEmpty(authType))
            {
                LaunchUtil.launchLogin(authType.ToLower(), false);
            }
        }

        [STAThread]
        public static async Task ShowLoggedInMessage()
        {
            MessageBox.Show("Successfully logged on to Code Time", "Code Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
