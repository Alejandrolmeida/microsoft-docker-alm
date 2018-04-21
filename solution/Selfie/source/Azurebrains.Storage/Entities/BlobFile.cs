using System;

namespace Azurebrains.Storage.Entities
{
    internal class BlobFile
    {
        #region Members
        private bool? _confirm;
        private string _name;
        private int? _typeDoc;
        private string _uriSas;
        #endregion

        #region Properties
        /// <summary>
        /// Optional.
        /// </summary>
        public bool? Confirm
        {
            get { return this._confirm; }
            set { this._confirm = value; }
        }       

        /// <summary>
        /// Optional.
        /// </summary>
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }        

        /// <summary>
        /// Optional.
        /// </summary>
        public int? TypeDoc
        {
            get { return this._typeDoc; }
            set { this._typeDoc = value; }
        }       

        /// <summary>
        /// Optional.
        /// </summary>
        public string UriSas
        {
            get { return this._uriSas; }
            set { this._uriSas = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the FileStorage class.
        /// </summary>
        public BlobFile()
        {
        }
        #endregion 
    }

}
