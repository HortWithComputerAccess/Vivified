using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Vivified
{
    [Plugin("Plugin Name")]
    public class Plugin
    {
        [Init]
        private void Init()
        {
            Debug.Log("Plugin has loaded!");
        }

        [Exit]
        private void Exit()
        {
            Debug.Log("Application has closed!");
        }
    }
}
