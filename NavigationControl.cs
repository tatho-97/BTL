using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTL
{
    internal class NavigationControl
    {
        List<UserControl> controls = new List<UserControl>();
        Panel panel;

        public NavigationControl(List<UserControl> controls, Panel panel)
        {
            this.controls = controls;
            this.panel = panel;
            AddUserControls();
        }

        private void AddUserControls()
        {
            for (int i = 0; i < controls.Count(); i++)
            {
                // set every UserControl's dock style to fill so that it will occupy the space inside the panel
                controls[i].Dock = DockStyle.Fill;
                // add all the UserControl inside the panel
                panel.Controls.Add(controls[i]);
            }
        }

        public void Display(int index)
        {
            if (index < controls.Count())
            {
                controls[index].BringToFront(); // display only the selected UserControl using index
            }
        }
    }
}
