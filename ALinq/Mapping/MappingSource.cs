using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents a source for mapping information.
    /// </summary>
    public abstract class MappingSource
    {
        // Fields
        private MetaModel primaryModel;
        private ReaderWriterLock rwlock;
        private Dictionary<Type, MetaModel> secondaryModels;

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MappingSource class.
        /// </summary>
        protected MappingSource()
        {
        }

        /// <summary>
        /// Creates a new mapping model.
        /// </summary>
        /// <param name="dataContextType">The type of ALinq.DataContext on which to base the mapping.</param>
        /// <returns>The meta-model created to match the current mapping scheme.</returns>
        protected abstract MetaModel CreateModel(Type dataContextType);

        /// <summary>
        /// Returns the mapping model.
        /// </summary>
        /// <param name="dataContextType">The type of ALinq.DataContext of the model to be returned.</param>
        /// <returns>The mapping model associated with this mapping source.</returns>
        public MetaModel GetModel(Type dataContextType)
        {
            MetaModel model2;
            if (dataContextType == null)
            {
                throw Error.ArgumentNull("dataContextType");
            }
            MetaModel model = null;
            if (this.primaryModel == null)
            {
                model = this.CreateModel(dataContextType);
                Interlocked.CompareExchange<MetaModel>(ref this.primaryModel, model, null);
            }
            if (this.primaryModel.ContextType == dataContextType)
            {
                return this.primaryModel;
            }
            if (this.secondaryModels == null)
            {
                Interlocked.CompareExchange(ref this.secondaryModels, new Dictionary<Type, MetaModel>(), null);
            }
            if (this.rwlock == null)
            {
                Interlocked.CompareExchange(ref this.rwlock, new ReaderWriterLock(), null);
            }
            this.rwlock.AcquireReaderLock(-1);
            try
            {
                if (this.secondaryModels.TryGetValue(dataContextType, out model2))
                {
                    return model2;
                }
            }
            finally
            {
                this.rwlock.ReleaseReaderLock();
            }
            this.rwlock.AcquireWriterLock(-1);
            try
            {
                if (this.secondaryModels.TryGetValue(dataContextType, out model2))
                {
                    return model2;
                }
                if (model == null)
                {
                    model = this.CreateModel(dataContextType);
                }
                this.secondaryModels.Add(dataContextType, model);
            }
            finally
            {
                this.rwlock.ReleaseWriterLock();
            }
            return model;
        }
    }



}
