using System.Linq.Expressions;
using System.Text.Json;

namespace PatchHanlde.JsonPatch
{
    public class Operation
    {
        public string Op { get; set; }
        public string Path { get; set; }
        public JsonElement Value { get; set; }

        private static Dictionary<string, object> _actions = new();

        public void Patch<T>(T model)
        {
            if (!Enum.TryParse<OperationType>(Op, true, out OperationType operationType))
                return;

            switch (operationType)
            {
                case OperationType.Replace:
                    var del = CreateDelegate<T>();
                    del(model, this);
                    break;

                case OperationType.Add:
                case OperationType.Test:
                case OperationType.Invalid:
                case OperationType.Copy:
                case OperationType.Move:
                case OperationType.Remove:
                default:
                    break;
            }
        }

        private Action<T, Operation> CreateDelegate<T>()
        {
            if (_actions.TryGetValue(typeof(T).FullName, out object action))
                return (Action<T, Operation>)action;

            var expression = CreateReplaceExpression<T>();
            var del = expression.Compile();
            _actions[typeof(T).FullName] = del;
            return del;
        }

        private Expression<Action<T, Operation>> CreateReplaceExpression<T>()
        {
            ParameterExpression input = Expression.Parameter(typeof(T), "input");
            var propertyName = GetPropertyName();
            var propertyInfo = typeof(T).GetProperties()
                .FirstOrDefault(p => string.Compare(p.Name, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException($"The property {Path} does not exist in {typeof(T).Name}");
            }

            if (!propertyInfo.CanWrite)
            {
                throw new InvalidOperationException($"The property {Path} in {typeof(T).Name} does not have a set accessor");
            }

            var accessProperty = Expression.MakeMemberAccess(input, propertyInfo);
            ParameterExpression operation = Expression.Parameter(typeof(Operation));
            var getValueMethod = typeof(Operation).GetMethod(nameof(GetValue),
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

            var value = Expression.Call(operation, getValueMethod, Expression.Constant(propertyInfo.PropertyType));

            //var assignment = Expression.Assign(accessProperty, 
            //    Expression.Convert(
            //        Expression.Constant(GetValue(propertyInfo.PropertyType, (JsonElement)Value)), propertyInfo.PropertyType));
            var assignment = Expression.Assign(accessProperty, 
                Expression.Convert(
                    value, propertyInfo.PropertyType));

            var lambda = Expression.Lambda<Action<T, Operation>>(assignment, input, operation);
            return lambda;
        }

        private object? GetValue(Type propertyType)
        {
            var value = (JsonElement)Value;
            object? res = propertyType.Name switch
            {
                "String" => value.GetString(),
                "Int32" => value.GetInt32(),
                _ => throw new InvalidOperationException($"Unsupported type: {propertyType.Name}"),
            };

            return res;
        }

        //private void PatchPerson(Person input)
        //{
        //    input.Name = null;// Value
        //}

        //private class Person
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //}
    

        private string GetPropertyName()
        {
            if (Path[0] != '/')
                throw new InvalidOperationException($"Path must begin with '/'");
            if (Path.IndexOf('/', 1) != -1)
                throw new InvalidOperationException($"Only single-level path are supported: {Path}");

            return Path.Substring(1);
        }

    }
}
