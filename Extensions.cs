using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace IcarusDataEditor
{
    public static class Extensions
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {

            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);

        }
    }
}
