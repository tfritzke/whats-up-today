using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WhatsUpToday.Core.Types.Entity;
using WhatsUpToday.Core.Data.Interfaces;

namespace WhatsUpToday.Core.Data.Services.Entity;

/*
 * This class is built over EF to abstract its operations on a
 * generic class.
 *
 * Any ORM can be used by implementing IEntityService and updating
 * the web registry.
 */

/*
 * Future ideas:
 * - Extend to manage a set of sets, SaveChanges{+} flushes them in
 *   parallel, number of sets per service is configurable.
 */
public abstract class EntityService<TEntity, TId> : IEntityService<TEntity, TId>
                            where TEntity : class, IEntity<TId>
                         // where TId : struct
{
    protected EntityService(IEFContextFactory factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        DbContext context = factory.CreateContext();
        Context = context ?? throw new ArgumentNullException("context");

        Set = new EntitySet();

        MaxOrdinal = 1;
    }

    ~EntityService()
    {
        // Do not change this code.
        // Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }


    // Types

    private class EntitySet : DbSet<TEntity>
    {
        public override IEntityType EntityType { get; }
    }

    // Data

    protected DbContext Context { get; private set; }
    protected DbSet<TEntity> Set { get; set; }

    // Methods

    /// <summary>
    /// Property for maximum number of sets
    /// </summary>
    public int MaxOrdinal { get; set; }

    /// <summary>
    /// Create an instance of the object type.
    /// </summary>
    /// <returns>One new object, unsaved.</returns>
    public TEntity CreateObject()
    {
        TEntity obj = Activator.CreateInstance<TEntity>();
        return obj;
    }


    // Context

    /* Note 1:
     * Invoking SaveChanges() on one service will affect all services that
     * share that context.
     * 
     * Note 2:
     * Each service's SaveChanges() should be called so that each can
     * save local data.
     */

    /// <summary>
    /// Save any local data before the service is disposed.
    /// </summary>
    private void LocalSaveChanges()
    {
    }

    public virtual int SaveChanges()
    {
        LocalSaveChanges();
        return Context.SaveChanges();
    }

    public virtual Task<int> SaveChangesAsync()
    {
        LocalSaveChanges();
        return Context.SaveChangesAsync();
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        LocalSaveChanges();
        return Context.SaveChangesAsync(cancellationToken);
    }

    // Entity Import/Export

#if docs
    // http://docs.telerik.com/help/openaccess-classic/openaccess-tasks-using-xmlserializer-with-generic-lists-and-persistent-objects.html
    void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
    {
        if (reader.IsEmptyElement || reader.Read() == false)
            return;
        XmlSerializer inner = new XmlSerializer(typeof(T));
        while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
        {
            T e = (T)inner.Deserialize(reader);
            src.Add(e);
        }
        reader.ReadEndElement();
    }

    void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
    {
        if (src.Count == 0)
            return;
        XmlSerializer inner = new XmlSerializer(typeof(T));
        for (int i = 0; i < src.Count; i++)
        {
            inner.Serialize(writer, src[i]);
        }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataToImport"></param>
    /// <returns></returns>
    public virtual bool ImportObjects(ref XDocument dataToImport)
    {
        if (dataToImport == null)
            throw new ArgumentNullException(nameof(dataToImport));

        //

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataExported"></param>
    /// <returns></returns>
    public virtual bool ExportObjects(ref XDocument dataExported)
    {
        string comment = string.Format("Data generated on {0}",
                                        DateTime.Now.ToUniversalTime().ToString());
#if docs
        var xEle = new XElement("List",
                    from cl in Set.AsEnumerable()
                    select new XElement("Item", cl.GetType()
                                                  .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                  .Select(p => new XElement(p.Name, p.GetValue(cl, null)))
                                        )
                                );
#endif

        string name = typeof(TEntity).Name;
        string nameList = "ListOf" + name;
        var xList = new XElement(nameList);
        bool rc;
        int count = 0;

        foreach (TEntity obj in Set)
        {
            XElement element = null;
            rc = false; // obj.ExportData(ref element);
            if (rc)
            {
                xList.Add(element);
                count++;
            }
        }
        xList.SetAttributeValue("size", count);

        dataExported = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XComment(comment),
                new XElement("Root"),
                xList
                );

        return true;
    }

    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).

                Set = null;
                Context.Dispose();
                Context = null;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }

    #endregion
}
