using AspectInjector.Broker;
using Reformat.Data.EFCore.Aspects.interfaces;

namespace Reformat.Data.EFCore.Aspects;

/// <summary>
/// AOP:事务处理
/// 20230529：支持嵌套事务，支持异常回滚
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Aspect(Scope.PerInstance)]
[Injection(typeof(TransactionAttribute))]
public class TransactionAttribute : Attribute
{
    private int TransHashCode;
    private Stack<int> callStack = new Stack<int>();

    [Advice(Kind.Before, Targets = Target.Method)]
    public void Before([Argument(Source.Instance)] object Instance)
    {
        ITransaction obj = Instance as ITransaction;
        if (obj.DbContext.Database.CurrentTransaction == null)
        {
            obj.DbContext.Database.BeginTransaction();
            TransHashCode = obj.DbContext.Database.CurrentTransaction.GetHashCode();
            Console.WriteLine("Transaction Begin: {0}", TransHashCode.ToString());
        }
        callStack.Push(1);
    }

    [Advice(Kind.After, Targets = Target.Method)]
    public void After([Argument(Source.Instance)] object Instance)
    {
        callStack.Pop();
        if (callStack.Count() == 0)
        {
            ITransaction obj = Instance as ITransaction;
            obj.DbContext.Database.CurrentTransaction.Commit();
            Console.WriteLine("Transaction Commit: {0}", TransHashCode.ToString());
        }
    }
    
    [Advice(Kind.Around, Targets = Target.Method)]
    public object RollBackHandle([Argument(Source.Instance)] object Instance,[Argument(Source.Target)] Func<object[], object> target, [Argument(Source.Arguments)] object[] args)
    {
        try
        {
            return target(args);
        }   
        catch (Exception ex)
        {
            ITransaction obj = Instance as ITransaction;
            if (callStack.Count() == 0)
            {
                obj.DbContext.Database.CurrentTransaction.Rollback();
                Console.WriteLine("Transaction RollBack: {0}", TransHashCode.ToString());
            }
            throw;
        }
    }
}