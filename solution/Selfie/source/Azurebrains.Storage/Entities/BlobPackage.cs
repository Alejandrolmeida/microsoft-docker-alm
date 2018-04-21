using System;
using System.Collections.Generic;

namespace Azurebrains.Storage.Entities
{
    internal class BlobPackage
    {
        #region Members
        private Guid _idPackage;
        private IList<BlobFile> _blobs;
        #endregion

        #region Properties
        public Guid IdPackage
        {
            get { return this._idPackage; }
        }
        public IList<BlobFile> Blobs
        {
            get { return this._blobs; }
            set { this._blobs = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BlobPackage class.
        /// </summary>
        public BlobPackage() 
            : this(new Guid())
        {            
        }
        /// <summary>
        /// Initializes a new instance of the BlobPackage class.
        /// </summary>
        /// <param name="idShipment">Unique ID Reference (in Guid object)</param>
        public BlobPackage(Guid idPackage)
        {
            _idPackage = idPackage;
            _blobs = new List<BlobFile>();
        }
        #endregion
    }
}
