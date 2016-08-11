using System;
using System.Collections.Generic;
using System.Text;

namespace MobileShareLibrary.GUIShare
{
    public interface ISharedGUICom
    {
        void EnableProcessingCover(string processMsg);
        void DisableProcessingCover();
        void ShowAlertMessage(string message);
    }
}
