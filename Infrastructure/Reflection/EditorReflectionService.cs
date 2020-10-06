using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using EZms.Core;
using EZms.Core.Attributes;
using EZms.Core.Models;
using EZms.UI.Areas.EZms.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace EZms.UI.Infrastructure.Reflection
{
    public class EditorReflectionService
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public EditorReflectionService(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
        }

        public List<ModelProperty> GetModelProperties(IContent content, out Type modelType, IFormCollection formCollection = null)
        {
            modelType = content.ModelType;

            var contentModel = JsonConvert.DeserializeObject(content.ModelAsJson, modelType);

            var properties = RecursiveModelProperties("", contentModel, formCollection: formCollection);

            return properties.OrderBy(w => w.Order).ThenBy(w => w.ReadOrder).ToList();
        }

        private List<ModelProperty> RecursiveModelProperties(string baseName, object model, PropertyInfo pi = null, IFormCollection formCollection = null, PropertyInfo[] propertyInfos = null)
        {
            baseName = baseName + (pi == null ? "" : $"{pi.Name}.");
            var modelProperties = new List<ModelProperty>();
            var properties = propertyInfos ?? (model == null ? pi.PropertyType.GetProperties() : model.GetType().GetProperties());

            properties = properties.Where(w => w.GetCustomAttribute<IgnorePropertyAttribute>() == null).ToArray();

            var propertyReadOrder = 0;
            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.CanWrite) continue;

                var isEnumerable = IsPropertyEnumerable(propertyInfo);
                if (propertyInfo.PropertyType.IsArray || isEnumerable)
                {
                    var value = model == null ? null : propertyInfo.GetValue(model);

                    if (value is IList arr/* && arr.Count != 0*/)
                    {
                        var arrayEntryType = arr.GetType().GetGenericArguments()[0];
                        //TODO: the handling of contentreference doesn't work when saving, only reading... why?!
                        if (arrayEntryType.IsPrimitive || arrayEntryType == typeof(string) || (formCollection == null && arrayEntryType == typeof(ContentReference)))
                            modelProperties.Add(CreateModelProperty(baseName,
                                GetFormCollectionValue(formCollection, propertyInfo, isEnumerable, value, baseName),
                                propertyInfo, propertyReadOrder));
                        else
                        {
                            if (arr.Count == 0)
                            {
                                var entry = GetDefault(arrayEntryType);
                                arr.Add(entry);
                            }

                            var property = CreateModelProperty(baseName, null, propertyInfo, propertyReadOrder);
                            var subProperties = new List<ModelProperty>();
                            for (var x = 0; x < arr.Count; x++)
                            {
                                subProperties.AddRange(RecursiveModelProperties($"{baseName}{property.Name}[{x}].",
                                    arr[x], propertyInfo, formCollection));
                            }

                            property.Properties = subProperties;
                            modelProperties.Add(property);
                        }
                    }
                    else
                    {
                        var v = GetDefault(propertyInfo.PropertyType) as IList;
                        var arrayEntryType = v.GetType().GetGenericArguments()[0];

                        if (arrayEntryType.IsPrimitive || arrayEntryType == typeof(string) || (formCollection == null && arrayEntryType == typeof(ContentReference)))
                            modelProperties.Add(CreateModelProperty(baseName, GetFormCollectionValue(formCollection, propertyInfo, isEnumerable, v, baseName), propertyInfo, propertyReadOrder));
                        else
                        {
                            var entry = GetDefault(arrayEntryType);
                            v.Add(entry);

                            var property = CreateModelProperty(baseName, null, propertyInfo, propertyReadOrder);
                            var subProperties = new List<ModelProperty>();
                            for (var x = 0; x < v.Count; x++)
                            {
                                subProperties.AddRange(RecursiveModelProperties($"{baseName}{property.Name}[{x}].",
                                    v[x], propertyInfo, formCollection));
                            }

                            property.Properties = subProperties.OrderBy(w => w.Order).ThenBy(w => w.ReadOrder).ToList();
                            modelProperties.Add(property);
                        }
                    }
                }
                else
                {
                    var value = model == null ? null : propertyInfo.GetValue(model);
                    var property = CreateModelProperty(baseName, GetFormCollectionValue(formCollection, propertyInfo, false, value, baseName), propertyInfo, propertyReadOrder);
                    if (!propertyInfo.PropertyType.IsPrimitive &&
                        propertyInfo.PropertyType != typeof(string) &&
                        propertyInfo.PropertyType != typeof(ContentReference))
                    {
                        var childProperties = RecursiveModelProperties(baseName, value, propertyInfo, formCollection);
                        property.Properties = childProperties.OrderBy(w => w.Order).ThenBy(w => w.ReadOrder).ToList();
                    }
                    modelProperties.Add(property);
                }

                propertyReadOrder++;
            }

            return modelProperties.OrderBy(w => w.Order).ThenBy(w => w.ReadOrder).ToList();
        }

        public void AddPropertiesToModel(IContent content, IFormCollection formCollection)
        {
            var modelType = content.ModelType;
            var contentModel = JsonConvert.DeserializeObject(content.ModelAsJson, modelType);
            var properties = contentModel.GetType().GetProperties().Where(w => w.GetCustomAttribute<IgnorePropertyAttribute>() == null);
            var modelProperties = RecursiveModelProperties("", contentModel, null, formCollection);

            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.CanWrite) continue;

                var isEnumerable = IsPropertyEnumerable(propertyInfo);
                var testProperty = modelProperties.FirstOrDefault(w => w.PropertyInfo == propertyInfo);
                if (testProperty != null)
                {
                    if (testProperty.IsComplexType)
                    {
                        if (testProperty.IsEnumerable)
                        {
                            var itemsType = testProperty.PropertyInfo.PropertyType.GenericTypeArguments.First();

                            var formGroups = formCollection.Keys.Where(w => w.StartsWith($"{testProperty.Name}["))
                                .Select(w => w.Split('.').First()).Distinct().ToList();
                            
                            var lst = new List<object>();
                            if (!formGroups.Any() && formCollection[testProperty.Name].Count > 0)
                            {
                                var converter = TypeDescriptor.GetConverter(itemsType);
                                lst.AddRange(formCollection[testProperty.Name].Where(key => !string.IsNullOrEmpty(key)).Select(key => converter.ConvertFrom(key)));
                            }
                            else
                            {
                                var itemProperties = itemsType.GetProperties().ToArray();
                                foreach (var key in formGroups)
                                {
                                    var formProperties = formCollection.Where(w => w.Key.StartsWith(key)).ToList();
                                    var subProperties = RecursiveModelProperties($"{key}.{propertyInfo.Name}.", null,
                                        null, new FormCollection(formProperties.ToDictionary(k => k.Key, v => v.Value)),
                                        itemProperties);

                                    lst.Add(CreateTypedPropertyValue(subProperties, itemsType));
                                }
                            }

                            propertyInfo.SetValue(contentModel, ConvertValueToType(lst, propertyInfo.PropertyType));
                        }
                        else
                        {
                            var obj = CreateTypedPropertyValue(testProperty.Properties, propertyInfo.PropertyType);
                            propertyInfo.SetValue(contentModel, obj);
                        }
                    }
                    else
                    {
                        var value = testProperty.Value;
                        if (!formCollection.ContainsKey(testProperty.Name))
                            value = GetDefault(testProperty.Type);

                        propertyInfo.SetValue(contentModel, ConvertValueToType(propertyInfo, isEnumerable, value));
                    }
                }
            }

            content.ModelAsJson = JsonConvert.SerializeObject(contentModel);
        }

        public static object GetDefault(Type type)
        {
            if (type == null) return null;

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            if (type.IsClass && type.GetConstructor(Type.EmptyTypes) != null)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        private ModelProperty CreateModelProperty(string baseName, object value, PropertyInfo propertyInfo, int propertyReadOrder)
        {
            var propertyMetadata = _modelMetadataProvider.GetMetadataForType(propertyInfo.PropertyType);
            var explorer = new ModelExplorer(_modelMetadataProvider, propertyMetadata, propertyInfo);
            var displayValues = propertyInfo.GetCustomAttributes<DisplayAttribute>().FirstOrDefault();
            var validationAttributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();
            var uiHint = propertyInfo.GetCustomAttributes<UIHintAttribute>().FirstOrDefault();
            var isEnumerable = IsPropertyEnumerable(propertyInfo);

            var property = new ModelProperty
            {
                GroupName = displayValues?.GroupName ?? "General",
                DisplayName = displayValues?.Name ?? propertyInfo.Name,
                Name = $"{baseName}{propertyInfo.Name}",
                Description = displayValues?.Description ?? "",
                Value = value,
                Order = displayValues?.GetOrder() ?? int.MaxValue,
                ValidationMessage = validationAttributes?.FirstOrDefault()?.ErrorMessage ?? "",
                ReadOrder = propertyReadOrder,
                Type = propertyInfo.PropertyType,
                PropertyInfo = propertyInfo,
                ModelExplorer = explorer,
                IsEnumerable = isEnumerable,
                UiHint = uiHint?.UIHint
            };

            return property;
        }

        private static bool IsPropertyEnumerable(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsArray || typeof(Enumerable).IsAssignableFrom(propertyInfo.PropertyType) ||
                   (propertyInfo.PropertyType != typeof(string) && propertyInfo.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)));
        }

        private static object CreateTypedPropertyValue(IEnumerable<ModelProperty> properties, Type propertyType)
        {
            var obj = new Expando();
            foreach (var property in properties)
            {
                if (property.IsComplexType)
                {
                    var subComplex = new Expando();
                    foreach (var subComplexProperty in property.Properties)
                    {
                        if (subComplexProperty.IsComplexType)
                        {
                            subComplex[subComplexProperty.Name.Split('.').Last()] = CreateTypedPropertyValue(subComplexProperty.Properties, subComplexProperty.Type);
                            continue;
                        }

                        subComplex[subComplexProperty.Name.Split('.').Last()] = string.IsNullOrEmpty(subComplexProperty.Value?.ToString()) ?
                            GetDefault(subComplexProperty.PropertyInfo.PropertyType) :
                            subComplexProperty.Value;
                    }

                    obj[property.Name.Split('.').Last()] = ConvertValueToType(subComplex.Properties, property.Type);
                }
                else
                    obj[property.Name.Split('.').Last()] = string.IsNullOrEmpty(property.Value?.ToString()) ? GetDefault(property.Type) : property.Value;
            }

            return ConvertValueToType(obj.Properties, propertyType);
        }

        private static object ConvertValueToType(object value, Type type)
        {
            var serialized = JsonConvert.SerializeObject(value);
            if (type == typeof(ContentReference))
            {
                var obj = JsonConvert.DeserializeObject(serialized, typeof(object)) as dynamic;
                return new ContentReference((int)obj.Id, (int)obj.WorkId);
            }

            return JsonConvert.DeserializeObject(serialized, type);
        }

        private static object ConvertValueToType(PropertyInfo property, bool isEnumerable, object value)
        {
            var propertyType = property.PropertyType;

            if (isEnumerable)
            {
                var enumerable = ((IList)value);
                var enumType = property.PropertyType.GetGenericArguments().FirstOrDefault();
                if (enumType != null && enumType != typeof(string))
                {
                    var listType = typeof(List<>);
                    var constructedListType = listType.MakeGenericType(enumType);
                    var list = (IList)Activator.CreateInstance(constructedListType);

                    if (enumerable == null) return list;

                    var itemConverter = TypeDescriptor.GetConverter(enumType);

                    foreach (var val in enumerable)
                    {
                        list.Add(itemConverter.ConvertFrom(val));
                    }

                    return list;
                }

                return enumerable;
            }

            if (value is string str && string.IsNullOrEmpty(str))
            {
                return GetDefault(propertyType);
            }

            var v = value ?? GetDefault(propertyType);
            if (v == null) return null;

            if (v.GetType() == propertyType)
                return v;

            var converter = TypeDescriptor.GetConverter(propertyType);
            return converter.ConvertFrom(v);
        }

        private static object GetFormCollectionValue(IFormCollection collection, PropertyInfo propertyInfo, bool isEnumerable, object defaultValue, string baseName)
        {
            if (collection == null) return ConvertValueToType(propertyInfo, isEnumerable, defaultValue);

            if (collection.TryGetValue($"{baseName}{propertyInfo.Name}", out var baseValues))
            {
                if (isEnumerable)
                    return ConvertValueToType(propertyInfo, true, baseValues.Where(w => !string.IsNullOrWhiteSpace(w)).Select(w => w).ToList());
                return ConvertValueToType(propertyInfo, false, baseValues.FirstOrDefault());
            }

            if (collection.TryGetValue(propertyInfo.Name, out var values))
            {
                if (isEnumerable)
                    return ConvertValueToType(propertyInfo, true, values.Where(w => !string.IsNullOrWhiteSpace(w)).Select(w => w).ToList());
                return ConvertValueToType(propertyInfo, false, values.FirstOrDefault());
            }

            return ConvertValueToType(propertyInfo, isEnumerable, defaultValue);
        }
    }
}
