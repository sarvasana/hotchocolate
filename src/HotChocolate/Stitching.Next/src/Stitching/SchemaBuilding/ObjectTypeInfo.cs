using System;
using System.Collections.Generic;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HotChocolate.Stitching.SchemaBuilding;

internal sealed class ObjectTypeInfo : ITypeInfo
{
    public ObjectTypeInfo(ObjectTypeDefinitionNode definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Name = definition.Name.Value;
    }

    public NameString Name { get; }

    public TypeKind Kind => TypeKind.Object;

    public ObjectTypeDefinitionNode Definition { get; set; }

    ITypeDefinitionNode ITypeInfo.Definition => Definition;

    public IList<ObjectFetcherInfo> Fetchers { get; } =
        new List<ObjectFetcherInfo>();

    public List<FieldSchemaBinding> Bindings { get; } =
        new List<FieldSchemaBinding>();
}