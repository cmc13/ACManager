using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ACManager.Settings
{
    public class BannedCharacterSettings
    {
        [XmlArray(ElementName = "BannedCharacters")]
        [XmlArrayItem(ElementName = "Name")]
        public List<BannedName> BannedCharacters = new List<BannedName>();

        [XmlArray(ElementName = "BannedMonarchs")]
        [XmlArrayItem(ElementName = "Name")]
        public List<BannedName> BannedMonarchs = new List<BannedName>();
    }

    public class BannedName
        : IEquatable<BannedName>
    {
        [XmlElement(IsNullable = false)]
        public string Name;

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BannedName);
        }

        public bool Equals(BannedName other)
        {
            return Name.Equals(other.Name);
        }
    }
}
