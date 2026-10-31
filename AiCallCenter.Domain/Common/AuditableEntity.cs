using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiCallCenter.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; protected set; }

    public Guid? CreatedBy { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    public Guid? UpdatedBy { get; protected set; }

    public DateTime? DeletedAt { get; protected set; }

    public Guid? DeletedBy { get; protected set; }

    public bool IsDeleted { get; protected set; }

    protected AuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}
