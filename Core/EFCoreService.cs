using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Reformat.Data.EFCore.Aspects.interfaces;
using Reformat.Data.EFCore.Core.interfaces;
using Reformat.Data.EFCore.Domain;
using Reformat.Framework.Core.Core;
using Reformat.Framework.Core.Exceptions;
using Reformat.Framework.Core.IOC.Attributes;
using Reformat.Framework.Core.IOC.Services;
using Reformat.Framework.Core.JWT.interfaces;
using Reformat.Framework.SqlSugar.Domain;

namespace Reformat.Data.EFCore.Core;

public abstract class EFCoreService<T> : BaseScopedService,ITransaction where T : EFCoreEntity
{
    [Autowired] public EFCoreDbContext DbContext { get; set; }
    
    [Autowired] protected IUserService UserService;

    public DbSet<T> EnttyDbSet => DbContext.GetDbSet<T>();
    
    public EFCoreService(IocScoped iocScoped) : base(iocScoped)
    {
    }

    public IQueryable<T> GetQueryable(bool isLogic)
    {
        IQueryable<T> queryable = EnttyDbSet.AsQueryable();
        if (isLogic) queryable = queryable.Where(item => item.IsDeleted == false);
        return queryable;
    }

    public List<T> GetList(bool isLogic = true) => GetQueryable(isLogic).ToList();
    
    
    public virtual async Task<PageList<T>> GetPage(SqlQuery<T> query) => await GetPage(query, GetQueryable(true));

    public virtual async Task<PageList<T>> GetPage(SqlQuery<T> query, IQueryable<T> queryable) => await BuildPage(query, queryable);
    
    public async Task<PageList<T>> BuildPage<T>(SqlQuery<T> query, IQueryable<T> queryable, bool isWhere = true, bool isOrder = true) where T : EFCoreEntity
    {
        if (isWhere) queryable = queryable.GetWhere(query);
        if (isOrder) queryable = queryable.GetOrder(query);

        PageList<T> pageList = new PageList<T>();
        
        pageList.Total = Count(queryable);

        int pageSize = query.PageSize;
        int pageIndex = query.PageIndex;
        if (pageSize == 0)
        {
            pageList.List = await queryable.ToListAsync();
        }
        else
        {
            pageList.List = await queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        return pageList;
    }


    public virtual int Count<T>(IQueryable<T> queryable) where T : EFCoreEntity => queryable.Count();


    public T GetById(long id,bool isLogic = true) => GetQueryable( isLogic).Single(i => i.Id == id);
    
    public List<T> GetByIds(long[] ids,bool isLogic = true) => GetQueryable( isLogic).Where(i => ids.Contains(i.Id)).ToList();
    
    public bool Save(ref T entity)
    {
        addUserInfo(ref entity);
        DbContext.Add(entity);
        return DbContext.SaveChanges() > 0;
    }

    public int SaveBatch(List<T> entities)
    {
        entities.ForEach(i => addUserInfo(ref i));
        DbContext.AddRange(entities);
        return DbContext.SaveChanges();
    }

    public bool UpdateById(T entity,bool ignoreNull = true)
    {
        updateUserInfo(ref entity);
        if (ignoreNull)
        {
            DbContext.Attach(entity);
            CheckPropertyUpdate(entity);
        }
        else
        {
            DbContext.Update(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
        }
        return DbContext.SaveChanges() > 0;
    }

    public int UpdateBatch(List<T> entities, bool ignoreNull = true)
    {
        entities.ForEach(i => updateUserInfo(ref i));
        if (ignoreNull)
        {
            DbContext.AttachRange(entities);
            entities.ForEach(i => CheckPropertyUpdate(i));
        }
        else
        {
            DbContext.UpdateRange(entities);
            DbContext.Entry(entities).State = EntityState.Modified;
        }
        return DbContext.SaveChanges();
    }

    private void CheckPropertyUpdate(T entity)
    {
        foreach (PropertyInfo p in entity.GetType().GetProperties())
        {
            if (p.GetValue(entity) != null)
            {
                bool result = p.GetCustomAttributes().Any(attr => attr is KeyAttribute);
                if(result) continue;
                DbContext.Entry<T>(entity).Property(p.Name).IsModified = true;
            }
        }
    }

    public bool DeleteById(long id,bool isLogic = true)
    {
        var entity = DbContext.Set<T>().Find(id);
        if (isLogic)
        {
            entity.IsDeleted = true;
        }
        else
        {
            DbContext.Remove(entity);
        }
        return DbContext.SaveChanges() > 0;
    }
    
    public int DeleteByIds(long[] ids, bool isLogic = true)
    {
        List<T> entities = DbContext.Set<T>().Where(e => ids.Contains(e.Id)).ToList();
        if (isLogic)
        {
            entities.ForEach(i => i.IsDeleted = true);
        }
        else
        {
            DbContext.RemoveRange(entities);
        }
        return DbContext.SaveChanges();
    }
    
    /// <summary>
    /// 获取当前用户
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PermissionException"></exception>
    public IUser GetCurrentUser()
    {
        IUser currentUser = UserService.GetCurrentUser();
        if (currentUser == null)
        {
            throw new PermissionException("当前用户状态异常");
        }

        return currentUser;
    }

    private void addUserInfo(ref T entity)
    {
        IRecordEntity recordInfo = entity as IRecordEntity;
        if (recordInfo != null)
        {
            IUser currentUser = GetCurrentUser();
            recordInfo.CreateBy = currentUser.Id;
            recordInfo.CreateTime = DateTime.Now;
            recordInfo.UpdateBy = currentUser.Id;
            recordInfo.UpdateTime = DateTime.Now;
        }
    }

    private void updateUserInfo(ref T entity)
    {
        IRecordEntity recordInfo = entity as IRecordEntity;
        if (recordInfo != null)
        {
            IUser currentUser = GetCurrentUser();
            recordInfo.UpdateBy = currentUser.Id;
            recordInfo.UpdateTime = DateTime.Now;
        }
    }
}