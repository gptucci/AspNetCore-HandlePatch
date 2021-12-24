using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace PatchHanlde.JsonPatch
{
    public record Operation(string Op, string Path, JsonElement Value = default, string From = "")
    {
        private static Dictionary<string, object?> _actions = new();
        private Func<Operation, Type, object> _onCustomConversion;
        private bool _alwaysUseCustomConversion;

        public void SetCustomConversionHook(Func<Operation, Type, object> onCustomConversion,
            bool useAlways = false)
        {
            _onCustomConversion = onCustomConversion;
            _alwaysUseCustomConversion = useAlways;
        }

        public void Patch<T>(T model)
        {
            var del = CreateDelegate<T>();
            del(model, this);
        }

        private Action<T, Operation> CreateDelegate<T>()
        {
            // modified so that the cache is also tied to the path
            var key = $"{typeof(T).FullName}.{Op}.{Path}";

            if (_actions.TryGetValue(key, out object? action))
            {
                if (action == null) throw new InvalidOperationException($"The key {key} contains a null reference");
                return (Action<T, Operation>)action;
            }

            var expression = CreateExpression<T>();
            var del = expression.Compile();
            _actions[key] = del;
            return del;
        }

        /// <summary>
        /// This method generates a lambda
        /// </summary>
        /// <typeparam name="T">Type of the model</typeparam>
        /// <returns>The Expression representing the lambda</returns>
        private Expression<Action<T, Operation>> CreateExpression<T>()
        {
            if (!Enum.TryParse(Op, true, out OperationType operationType))
                throw new InvalidOperationException($"Invalid operation type {Op}");

            // first incoming parameter of the lambda
            ParameterExpression input = Expression.Parameter(typeof(T), "input");

            // second incoming parameter of the lambda
            // This is needed to access Path and Value
            ParameterExpression operation = Expression.Parameter(typeof(Operation));

            var body = operationType switch
            {
                OperationType.Replace => CreateOperandsForReplace(input, operation),
                OperationType.Add => CreateOperandsForAdd(input, operation),
                OperationType.Remove => CreateOperandsForRemove(input, operation),
                OperationType.Copy => CreateOperandsForCopy(input, operation),
                OperationType.Move => CreateOperandsForMove(input, operation),
                OperationType.Test => CreateOperandsForTest(input, operation),

                _ => throw new Exception($"Unsupported operation {Op}"),
            };

            // Build the final lambda
            var lambda = Expression.Lambda<Action<T, Operation>>(body, input, operation);
            return lambda;
        }

        // feature complete
        private Expression CreateOperandsForReplace(Expression input, Expression operation)
        {
            // Build the complete access path from the Path syntax
            var (target, pathFlags) = BuildFromPath(input, Path);
            if (!pathFlags.HasFlag(PathFlags.CanWrite))
            {
                throw new InvalidOperationException($"The property pointed from {Path} cannot be written");
            }

            // The Expression representing "operation.Value" (it is a System.Text.Json.JsonElement)
            var valueProp = Expression.Property(operation, "Value");

            // Build the Expression using the most appropriate GetXXX method on JsonElement
            // The method is choosen from the type of the property pointed from the Path
            var getValue = GetGetValueExpression(valueProp, target.Type);

            // Assign the Value retrieved from the JsonElement to the property
            var assignment = Expression.Assign(target, getValue);
            return assignment;
        }

        // not completed
        private Expression CreateOperandsForAdd(Expression input, Expression operation)
        {
            // Build the complete access path from the Path syntax
            var (target, pathFlags) = BuildFromPath(input, Path);
            if (pathFlags.HasFlag(PathFlags.IsArray))
            {
                throw new InvalidOperationException($"The operation type 'Add' is not supported on arrays. Consider changing the property to a collection");
            }

            // if the the target is an object, we treat it as a replace
            // TODO: is the Value syntax the same of replace or should we support the more "complex" object syntax?
            if (pathFlags.HasFlag(PathFlags.IsObject))
            {
                if (!pathFlags.HasFlag(PathFlags.CanWrite))
                {
                    throw new InvalidOperationException($"The property pointed from {Path} cannot be written");
                }

                // The Expression representing "operation.Value" (it is a System.Text.Json.JsonElement)
                var valueProp = Expression.Property(operation, "Value");

                // Build the Expression using the most appropriate GetXXX method on JsonElement
                // The method is choosen from the type of the property pointed from the Path
                var getValue = GetGetValueExpression(valueProp, target.Type);

                // Assign the Value retrieved from the JsonElement to the property
                var assignment = Expression.Assign(target, getValue);
            }

            // if the the target is a collection, we have to invoke the 'InsertAt' (if there is an index) or Add (if there is '-')
            if (pathFlags.HasFlag(PathFlags.IsCollection))
            {
                //TODO
            }

            throw new NotSupportedException($"The operation is not supported on flags {pathFlags}");
        }

        // feature complete
        private Expression CreateOperandsForRemove(Expression input, Expression operation)
        {
            // Build the complete access path from the Path syntax
            var (target, pathFlags) = BuildFromPath(input, Path);
            if (pathFlags.HasFlag(PathFlags.IsArray))
            {
                throw new InvalidOperationException($"The operation type 'Remove' is not supported on arrays. Consider changing the property to a collection");
            }

            // remove on object is done by assigning the default value
            if (pathFlags.HasFlag(PathFlags.IsObject))
            {
                if (!pathFlags.HasFlag(PathFlags.CanWrite))
                {
                    throw new InvalidOperationException($"The property pointed from {Path} cannot be written");
                }

                var assignment = Expression.Assign(target, Expression.Default(target.Type));
                return assignment;
            }

            // if the the target is a collection, we have to invoke the 'Remove' method passing the item to remove
            if (pathFlags.HasFlag(PathFlags.IsCollection))
            {
                var arrayIndex = target as IndexExpression;
                var collection = arrayIndex?.Object;
                var removeMethod = collection?.Type.GetMethod("Remove");
                if (collection == null || removeMethod == null)
                    throw new InvalidOperationException($"The collection does not support removing the item {Path}");
                return Expression.Call(collection, removeMethod, target);
            }

            throw new NotSupportedException($"The operation is not supported on flags {pathFlags}");
        }

        // TODO
        private Expression CreateOperandsForCopy(Expression input, Expression operation)
        {
            // Build the complete access path from the Path syntax
            var (source, fromPathFlags) = BuildFromPath(input, From);

            // Build the complete access path from the Path syntax
            var (target, targetPathFlags) = BuildFromPath(input, Path);


            throw new NotSupportedException($"The operation is not supported on flags {targetPathFlags}");
        }

        private Expression CreateOperandsForMove(Expression input, Expression operation)
        {
            throw new NotSupportedException();
        }

        private Expression CreateOperandsForTest(Expression input, Expression operation)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Build the Expression using the most appropriate GetXXX method on JsonElement
        /// </summary>
        /// <param name="jsonElement">The Expression representing the JsonElement</param>
        /// <param name="type">The type expected from the property pointed from Path</param>
        /// <returns>The Expression returning the value</returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="Exception"></exception>
        private Expression GetGetValueExpression(Expression jsonElement, Type type)
        {
            if (!_alwaysUseCustomConversion)
            {
                Type jet = typeof(JsonElement);
                if (type == typeof(Guid)) return BuildCall(jsonElement, jet, "GetGuid");
                if (type == typeof(string)) return BuildCall(jsonElement, jet, "GetString");
                if (type == typeof(bool)) return BuildCall(jsonElement, jet, "GetBoolean");
                if (type == typeof(byte)) return BuildCall(jsonElement, jet, "GetByte");
                if (type == typeof(sbyte)) return BuildCall(jsonElement, jet, "GetSByte");
                if (type == typeof(DateTime)) return BuildCall(jsonElement, jet, "GetDateTime");
                if (type == typeof(DateTimeOffset)) return BuildCall(jsonElement, jet, "GetDateTimeOffset");
                if (type == typeof(Decimal)) return BuildCall(jsonElement, jet, "GetDecimal");
                if (type == typeof(Double)) return BuildCall(jsonElement, jet, "GetDouble");
                if (type == typeof(Single)) return BuildCall(jsonElement, jet, "GetSingle");
                if (type == typeof(Int16)) return BuildCall(jsonElement, jet, "GetInt16");
                if (type == typeof(Int32)) return BuildCall(jsonElement, jet, "GetInt32");
                if (type == typeof(Int64)) return BuildCall(jsonElement, jet, "GetInt64");
                if (type == typeof(UInt16)) return BuildCall(jsonElement, jet, "GetUInt16");
                if (type == typeof(UInt32)) return BuildCall(jsonElement, jet, "GetUInt32");
                if (type == typeof(UInt64)) return BuildCall(jsonElement, jet, "GetUInt64");
            }

            if (_onCustomConversion != null)
            {
                return Expression.Convert(
                        Expression.Invoke(Expression.Constant(_onCustomConversion),
                                        Expression.Constant(this),
                                        Expression.Constant(type)),
                        type);
            }

            throw new NotSupportedException($"The type {type.Name} is not supported");

            static Expression BuildCall(Expression instance, Type type, string methodName)
            {
                var methodInfo = type.GetMethod(methodName);
                if (methodInfo == null) throw new Exception($"Cannot find {methodName} in type {type.Name}");
                return Expression.Call(instance, methodInfo);
            }
        }

        /// <summary>
        /// Build the complete access path from the Path syntax
        /// Path can be complex (multiple '/' specifying numeric indexes as well)
        /// </summary>
        /// <param name="root">The root Expression to start from</param>
        /// <param name="path">The path to process. It can be either Path or From</param>
        /// <returns>The accessor as an Expression</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private (Expression expression, PathFlags flags) BuildFromPath(Expression root, string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            Expression temp = root;
            PropertyInfo? lastProperty = null;
            PathFlags flags = PathFlags.None;
            foreach (var segment in segments)
            {
                if (char.IsNumber(segment[0]))
                {
                    if (!int.TryParse(segment, out int index))
                    {
                        throw new InvalidOperationException($"The segment {segment} in the path {path} is not valid");
                    }

                    if (temp.Type.IsArray)
                    {
                        // It is an array
                        temp = Expression.ArrayAccess(temp, Expression.Constant(index));
                        flags |= PathFlags.CanWrite;
                        flags |= PathFlags.IsArray;
                        flags &= ~PathFlags.IsCollection;
                        flags &= ~PathFlags.IsObject;
                    }
                    else if (typeof(System.Collections.ICollection).IsAssignableFrom(temp.Type))
                    {
                        // it is a collection accessible with the indexer (implements ICollection)
                        var itemProperty = temp.Type.GetProperty("Item", new Type[] { typeof(int) });
                        if (itemProperty == null)
                        {
                            throw new InvalidOperationException($"Cannot find a valid indexer in property {lastProperty?.Name}");
                        }

                        if (itemProperty.CanWrite)
                            flags |= PathFlags.CanWrite;
                        else
                            flags &= ~PathFlags.CanWrite;
                        flags &= ~PathFlags.IsArray;
                        flags |= PathFlags.IsCollection;
                        flags &= ~PathFlags.IsObject;
                        temp = Expression.Property(temp, itemProperty, Expression.Constant(index));
                    }

                }
                else
                {
                    // it is a standard property
                    var currentType = temp.Type;
                    lastProperty = currentType.GetProperties()
                        .FirstOrDefault(p => string.Compare(p.Name, segment, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (lastProperty == null)
                    {
                        throw new InvalidOperationException($"The path {path} does not exist in the Type {root.Type.Name}");
                    }

                    if (lastProperty.CanWrite)
                        flags |= PathFlags.CanWrite;
                    else
                        flags &= ~PathFlags.CanWrite;
                    flags &= ~PathFlags.IsArray;
                    flags &= ~PathFlags.IsCollection;
                    flags |= PathFlags.IsObject;
                    temp = Expression.MakeMemberAccess(temp, lastProperty);
                }

            }

            return (temp, flags);
        }

    }
}
