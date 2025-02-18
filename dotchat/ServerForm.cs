using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotchat
{
    public partial class ServerForm : BaseForm
    {
        public ServerForm()
        {
            InitializeComponent();

            SetChatList(lstChat);
        }
    }
}
