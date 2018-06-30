﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class ArmorSetViewModel : ViewModelBase
    {
        private IArmorPiece[] armorPieces;
        public IArmorPiece[] ArmorPieces
        {
            get { return armorPieces; }
            set { SetValue(ref armorPieces, value); }
        }
    }
}