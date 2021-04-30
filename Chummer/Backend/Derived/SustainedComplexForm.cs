/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using Chummer.Backend.Skills;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Sustained Complex Form
    /// </summary>
    
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class SustainedComplexForm : ComplexForm, ISustainable, IHasInternalId, IHasName, IHasSource
    {
        private bool _blnSelfSustained = true;
        private int _intForce = 0;
        private int _intNetHits = 0;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public SustainedComplexForm(Character objCharacter) : base(objCharacter)
        {
            //Create the GUID for new sustained Complex Forms
            guiID = Guid.NewGuid();
        }

        /// <summary>
        /// Creates an SustainedComplexForm from an provided ComplexForm 
        /// </summary>
        /// <param name="formRef">A Complex form the sustained Form is based upon</param>
        public void Create(ComplexForm formRef)
        {
            guiSourceID = formRef.SourceID;
            Name = formRef.Name;
        }

        /// <summary>
        ///  Save the objects xml to the XmlWriter.
        /// </summary>
        /// <param name="objWriter"></param>
        public override void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("type", nameof(SustainedComplexForm));
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", strName);
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Loads an SustainedComplexForm from XML
        /// </summary>
        /// <param name="objNode"></param>
        public override void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out guiID))
            {
                guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref strName);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref guiSourceID);
            }
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetInt32FieldQuickly("nethits", ref _intNetHits);
            objNode.TryGetBoolFieldQuickly("self", ref _blnSelfSustained);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter"></param>
        /// <param name="objCulture">Unused, needed to satisfy the Interface</param>
        /// <param name="strLanguageToPrint"></param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            //Do not change the Start Element name!
            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("type", nameof(SustainedSpell));
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalOptions.InvariantCultureInfo));

            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        public int NetHits
        {
            get => _intNetHits;
            set => _intNetHits = value;
        }

        public int Force
        {
            get => _intForce;
            set => _intForce = value;
        }

        public bool SelfSustained
        {
            get => _blnSelfSustained;
            set
            {
                if (_blnSelfSustained != value)
                {
                    _blnSelfSustained = value;
                    objCharacter.OnPropertyChanged();
                }

            }

        }
        #endregion
    }
}
