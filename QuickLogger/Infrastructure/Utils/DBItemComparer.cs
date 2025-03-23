using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.Utils;

public class DBItemComparer : IEqualityComparer<DBItem>
{
    public bool Equals(DBItem x, DBItem y)
    {
        return x.Id == y.Id;
    }

    public int GetHashCode(DBItem obj)
    {
        return obj.Id.GetHashCode();
    }
}

