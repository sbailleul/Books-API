using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Books.Api
{
    public class ArrayModelBinder: IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).Values.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var enumerableType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(enumerableType);

            var values = value.Split(':', StringSplitOptions.RemoveEmptyEntries).
                Select(v => converter.ConvertFromString(v.Trim())).
                ToArray();
            var typedValues = Array.CreateInstance(enumerableType, values.Length);
            values.CopyTo(typedValues, 0);

            bindingContext.Model = typedValues;
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
