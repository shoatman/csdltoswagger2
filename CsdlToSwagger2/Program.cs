// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Microsoft.Identity.Experiments.CsdlToSwagger2
{
    public static class ExtensionMethods
    {
        public static JObject Responses(this JObject jObject, JObject responses)
        {
            jObject.Add("responses", responses);

            return jObject;
        }

        public static JObject ResponseRef(this JObject responses, string name, string description, string refType)
        {
            responses.Add(name, new JObject()
            {
                {"description", description},
                {
                    "schema", new JObject()
                    {
                        {"$ref", refType}
                    }
                }
            });

            return responses;
        }

        public static JObject Response(this JObject responses, string name, string description, IEdmType type)
        {
            var schema = new JObject();
            Program.SetSwaggerType(schema, type);

            responses.Add(name, new JObject()
            {
                {"description", description},
                {"schema", schema}
            });

            return responses;
        }

        public static JObject ResponseArrayRef(this JObject responses, string name, string description, string refType)
        {
            responses.Add(name, new JObject()
            {
                {"description", description},
                {
                    "schema", new JObject()
                    {
                        {"type", "array"},
                        {
                            "items", new JObject()
                            {
                                {"$ref", refType}
                            }
                        }
                    }
                }
            });

            return responses;
        }

        public static JObject DefaultErrorResponse(this JObject responses)
        {
            return responses.ResponseRef("default", "Unexpected error", "#/definitions/_Error");
        }

        public static JArray DefaultAuthorizationParameter(this JArray parameters)
        {
            parameters.Add(new JObject()
            {
                {"name", "Authorization"},
                {"in", "header"},
                {"description", "Specify the bearer token used to authorize access to this API"},
                {"type", "string"},
            });

            return parameters;
        }

        public static JObject Response(this JObject responses, string name, string description)
        {
            responses.Add(name, new JObject()
            {
                {"description", description},
            });

            return responses;
        }

        public static JObject Parameters(this JObject jObject, JArray parameters)
        {
            jObject.Add("parameters", parameters);

            return jObject;
        }

        public static JArray Parameter(this JArray parameters, string name, string kind, string description, string type, string format = null)
        {
            parameters.Add(new JObject()
            {
                {"name", name},
                {"in", kind},
                {"description", description},
                {"type", type},
            });

            if (!string.IsNullOrEmpty(format))
            {
                (parameters.First as JObject).Add("format", format);
            }


            return parameters;
        }

        public static JArray Parameter(this JArray parameters, string name, string kind, string description, IEdmType type)
        {
            var parameter = new JObject()
            {
                {"name", name},
                {"in", kind},
                {"description", description},
            };

            if (kind != "body")
            {
                Program.SetSwaggerType(parameter, type);
            }
            else
            {
                var schema = new JObject();
                Program.SetSwaggerType(schema, type);
                parameter.Add("schema", schema);
            }

            parameters.Add(parameter);

            return parameters;
        }

        public static JArray ParameterRef(this JArray parameters, string name, string kind, string description, string refType)
        {
            parameters.Add(new JObject()
            {
                {"name", name},
                {"in", kind},
                {"description", description},
                {
                    "schema", new JObject()
                    {
                        {"$ref", "#/definitions/" + refType}
                    }
                }
            });

            return parameters;
        }

        public static JObject Tags(this JObject jObject, params string[] tags)
        {
            jObject.Add("tags", new JArray(tags));

            return jObject;
        }

        public static JObject Summary(this JObject jObject, string summary)
        {
            jObject.Add("summary", summary);

            return jObject;
        }

        public static JObject Description(this JObject jObject, string description)
        {
            jObject.Add("description", description);

            return jObject;
        }

        public static JObject OperationId(this JObject jObject, params string[] ops)
        {
            StringBuilder sb = new StringBuilder();
            TextInfo ti = CultureInfo.CurrentUICulture.TextInfo;

            for (int i = 0; i < ops.Length; i++)
            {
                sb.Append(ti.ToTitleCase(ops[i]));
            }

            jObject.Add("operationId", sb.ToString());

            return jObject;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        /// <summary>
        /// Parameters controlling the swagger generation
        /// </summary>
        private const string metadataURI = @"C:\Users\shoatman\Documents\Visual Studio 2017\Projects\CsdlToSwagger2\CsdlToSwagger2\msgraph.xml";
        private const string host = "graph.microsoft.com";
        private const string version = "1.0";
        private const string basePath = "/v1.0";
        private const string outputFile = @"C:\Users\shoatman\Documents\Visual Studio 2017\Projects\CsdlToSwagger2\CsdlToSwagger2\msgraph.json";

        /// <summary>
        /// Returns the swagger API Operations (Get, Post) associated with an Entity Set (Collection)
        /// </summary>
        /// <param name="entitySet"></param>
        /// <returns></returns>
        static JObject CreatePathItemObjectForEntitySet(IEdmEntitySet entitySet)
        {
            return new JObject()
            {
                {
                    "get", new JObject()
                        .Summary("Get EntitySet " + entitySet.Name)
                        .Description("Returns the EntitySet " + entitySet.Name)
                        .OperationId("Get", entitySet.Name)
                        .Tags(entitySet.Name)
                        .Parameters(new JArray()
                            .Parameter("$search", "query", "Search criteria; name value pair seprate by a colon", "string")
                            .Parameter("$filter", "query", "Filter the response based on one or more criteria", "string")
                            .Parameter("$expand", "query", "Expand navigation property", "string")
                            .Parameter("$select", "query", "select structural property", "string")
                            .Parameter("$orderby", "query", "order by some property", "string")
                            .Parameter("$top", "query", "top elements", "integer")
                            .Parameter("$skip", "query", "skip elements", "integer")
                            .Parameter("$skipToken", "query", "Paging token that is used to get the next or previous set of results", "string")
                            .Parameter("previous-page", "query", "When paging results indicates whether you want the previous page of results", "boolean")
                            .Parameter("$count", "query", "include count in response", "boolean")
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType())
                            .DefaultErrorResponse()
                        )

                },
                {
                    "post", new JObject()
                        .Summary("Post a new entity to EntitySet " + entitySet.Name)
                        .Description("Post a new entity to EntitySet " + entitySet.Name)
                        .OperationId("Add", entitySet.Name)
                        .Tags(entitySet.Name)
                        .Parameters(new JArray()
                            .Parameter(entitySet.EntityType().Name, "body", "The entity to post",
                                entitySet.EntityType())
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType())
                            .DefaultErrorResponse()
                        )
                }
            };
        }

        static JObject CreatePathItemObjectForNavigationPropertyMultipleEntitySet(IEdmNavigationProperty navProp)
        {
            IEdmEntityType targetEntity = navProp.ToEntityType();
            return new JObject()
            {
                {
                    "get", new JObject()
                        .Summary("Get navigation property collection " + navProp.Name)
                        .Description("Returns the  " + navProp.Name + " collection")
                        .OperationId("Get", navProp.DeclaringEntityType().Name, navProp.Name)
                        .Parameters(new JArray()
                            .Parameter("id", "query", "the identifier of the parent object", "string")
                            .Parameter("$search", "query", "Search criteria; name value pair seprate by a colon", "string")
                            .Parameter("$filter", "query", "Filter the response based on one or more criteria", "string")
                            .Parameter("$expand", "query", "Expand navigation property", "string")
                            .Parameter("$select", "query", "select structural property", "string")
                            .Parameter("$orderby", "query", "order by some property", "string")
                            .Parameter("$top", "query", "top elements", "integer")
                            .Parameter("$skip", "query", "skip elements", "integer")
                            .Parameter("$skipToken", "query", "Paging token that is used to get the next or previous set of results", "string")
                            .Parameter("previous-page", "query", "When paging results indicates whether you want the previous page of results", "boolean")
                            .Parameter("$count", "query", "include count in response", "boolean")
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + navProp.Name, targetEntity)
                            .DefaultErrorResponse()
                        )
                }
            };
        }

        static JObject CreatePathItemObjectForNavigationPropertyEntitySetAdd(IEdmNavigationProperty navProp)
        {
            IEdmEntityType targetEntity = navProp.ToEntityType();
            return new JObject()
            {
                {
                    "post", new JObject()
                        .Summary("Post a new ref to the navigation property collection " + navProp.Name)
                        .Description("Post a new entity to EntitySet " + navProp.Name)
                        .OperationId("Add", navProp.DeclaringEntityType().Name, navProp.Name)
                        .Tags(navProp.Name)
                        .Parameters(new JArray()
                            .Parameter("id", "query", "the identifier of the parent object", "string")
                            .ParameterRef("ref", "body", "an object identifying the object to refer to", "_ref")
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("204", "Empty response")
                            .DefaultErrorResponse()
                        )
                }
            };
        }

        static JObject CreatePathItemObjectForNavigationPropertyEntitySetRemove(IEdmNavigationProperty navProp)
        {
            IEdmEntityType targetEntity = navProp.ToEntityType();
            return new JObject()
            {
                {
                    "delete", new JObject()
                        .Summary("Delete a ref from the navigation property collection " + navProp.Name)
                        .Description("Delete a ref from the navigation property collection " + navProp.Name)
                        .OperationId("Remove", navProp.DeclaringEntityType().Name, navProp.Name)
                        .Tags(navProp.Name)
                        .Parameters(new JArray()
                            .Parameter("id", "query", "the identifier of the parent object", "string")
                            .Parameter("refId", "query", "the identifier of the object you want to remove", "string")
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("204", "Empty response")
                            .DefaultErrorResponse()
                        )
                }
            };
        }


        /// <summary>
        /// Returns the path item object that corresponds to a resource/entity/element
        /// </summary>
        /// <param name="entitySet"></param>
        /// <returns></returns>
        static JObject CreatePathItemObjectForEntity(IEdmEntitySet entitySet)
        {
            var keyParameters = new JArray();
            foreach (var key in entitySet.EntityType().Key())
            {
                string format;
                string type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                keyParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            return new JObject()
            {
                {
                    "get", new JObject()
                        .Summary("Get entity from " + entitySet.Name + " by key.")
                        .Description("Returns the entity with the key from " + entitySet.Name)
                        .OperationId("Get", entitySet.EntityType().Name)
                        .Tags(entitySet.Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .Parameter("$select", "query", "description", "string")
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType())
                            .DefaultErrorResponse()
                        )
                },
                {
                    "patch", new JObject()
                        .Summary("Update entity in EntitySet " + entitySet.Name)
                        .Description("Update entity in EntitySet " + entitySet.Name)
                        .OperationId("Update", entitySet.EntityType().Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .Parameter(entitySet.EntityType().Name, "body", "The entity to patch",
                                entitySet.EntityType())
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("204", "Empty response")
                            .DefaultErrorResponse()
                        )
                },
                {
                    "delete", new JObject()
                        .Summary("Delete entity in EntitySet " + entitySet.Name)
                        .Description("Delete entity in EntitySet " + entitySet.Name)
                        .OperationId("Delete", entitySet.EntityType().Name)
                        .Tags(entitySet.Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .Parameter("If-Match", "header", "If-Match header", "string")
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("204", "Empty response")
                            .DefaultErrorResponse()
                        )
                }
            };
        }

        static JObject CreatePathItemObjectForNavigationPropertyEntity(IEdmNavigationProperty navProp)
        {
            var keyParameters = new JArray();
            foreach (var key in navProp.ToEntityType().Key())
            {
                string format;
                string type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                keyParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            return new JObject()
            {
                {
                    "get", new JObject()
                        .Summary("Get entity from " + navProp.Name + " by key.")
                        .Description("Returns the entity with the key from " + navProp.Name)
                        .OperationId("Get", navProp.DeclaringEntityType().Name, navProp.Name)
                        .Tags(navProp.Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .Parameter("id", "query", "The identifier of the object", "string")
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + navProp.Name, navProp.ToEntityType())
                            .DefaultErrorResponse()
                        )
                }
            };
        }

        static JObject CreatePathItemObjectForNavigationPropertyZeroOrOne(IEdmNavigationProperty navProp)
        {
            var keyParameters = new JArray();
            foreach (var key in navProp.ToEntityType().Key())
            {
                string format;
                string type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                keyParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            return new JObject()
            {
                {
                    "get", new JObject()
                        .Summary("Get entity from " + navProp.Name + " by key.")
                        .Description("Returns the entity with the key from " + navProp.Name)
                        .OperationId("Get", navProp.DeclaringEntityType().Name, navProp.Name)
                        .Tags(navProp.Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .DefaultAuthorizationParameter()
                        )
                        .Responses(new JObject()
                            .Response("200", "Entity " + navProp.Name, navProp.ToEntityType())
                            .DefaultErrorResponse()
                        )
                }
            };
        }


        /// <summary>
        /// Returns the path (uri) associated with a collection and/or element
        /// </summary>
        /// <param name="entitySet"></param>
        /// <returns></returns>
        static string GetPathForEntity(IEdmEntitySet entitySet)
        {
            string singleEntityPath = "/" + entitySet.Name;
            var key = entitySet.EntityType().Key().First();
            singleEntityPath += "/{" + key.Name + "}";
            return singleEntityPath;
        }

        static string GetPathForEntityProperty(IEdmEntitySet entitySet)
        {
            string singleEntityPath = "/" + entitySet.Name;
            singleEntityPath += "/{propertyName}";
            return singleEntityPath;
        }

        static string GetPathForEntityNavigationPropertyEntity(string entityPath, IEdmNavigationProperty navProp)
        {
            entityPath = entityPath + "/" + navProp.Name + "/{refId}";
            return entityPath;
        }

        static string GetPathForEntityNavigationPropertyEntityAdd(string entityPath, IEdmNavigationProperty navProp)
        {
            entityPath = entityPath + "/" + navProp.Name + "/$ref";
            return entityPath;
        }
        static string GetPathForEntityNavigationPropertyEntityRemove(string entityPath, IEdmNavigationProperty navProp)
        {
            var key = navProp.ToEntityType().Key().First();
            entityPath = entityPath + "/" + navProp.Name + "/{refId}/$ref";
            return entityPath;
        }

        static string GetPathForEntityNavigationPropertyEntitySet(string entityPath, IEdmNavigationProperty navProp)
        {
            var key = navProp.ToEntityType().Key().First();
            entityPath = entityPath + "/" + navProp.Name + "/";
            return entityPath;
        }

        static JObject CreateSwaggerPathForOperationImport(IEdmOperationImport operationImport)
        {

            Debug.Print("CreateSwaggerPathForOperationImport-Operation Name: " + operationImport.Name);

            JArray swaggerParameters = new JArray();
            foreach (var parameter in operationImport.Operation.Parameters)
            {
                swaggerParameters.Parameter(parameter.Name, operationImport is IEdmFunctionImport ? "path" : "body",
                    "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            JObject swaggerResponses = new JObject();
            if (operationImport.Operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operationImport.Name,
                    operationImport.Operation.ReturnType.Definition);
            }

            JObject swaggerOperationImport = new JObject()
                .Summary("Call operation import  " + operationImport.Name)
                .Description("Call operation import  " + operationImport.Name)
                .Tags(operationImport is IEdmFunctionImport ? "Function Import" : "Action Import");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperationImport.Parameters(swaggerParameters);
            }
            swaggerOperationImport.Responses(swaggerResponses.DefaultErrorResponse());

            return new JObject()
                {
                    {operationImport is IEdmFunctionImport ? "get" : "post", swaggerOperationImport}
                };
        }

        static JObject CreateSwaggerPathForOperationOfEntitySet(IEdmOperation operation, IEdmEntitySet entitySet)
        {
            JArray swaggerParameters = new JArray();
            foreach (var parameter in operation.Parameters.Skip(1))
            {
                swaggerParameters.Parameter(parameter.Name, operation is IEdmFunction ? "path" : "body",
                    "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            JObject swaggerResponses = new JObject();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name,
                    operation.ReturnType.Definition);
            }

            Debug.Print("CreateSwaggerPathForOperationOfEntitySet-Operation Name: " + operation.Name);

            JObject swaggerOperation = new JObject()
                .Summary("Call operation  " + operation.Name)
                .Description("Call operation  " + operation.Name)
                .Tags(entitySet.Name, operation is IEdmFunction ? "Function" : "Action");


            if (swaggerParameters.Count > 0)
            {
                swaggerOperation.Parameters(swaggerParameters);
            }
            swaggerOperation.Responses(swaggerResponses.DefaultErrorResponse());
            return new JObject()
                {
                    {operation is IEdmFunction ? "get" : "post", swaggerOperation}
                };
        }

        static JObject CreateSwaggerPathForOperationOfEntity(IEdmOperation operation, IEdmEntitySet entitySet)
        {
            Debug.Print("CreateSwaggerPathForOperationOfEntity-Operation Name: " + operation.Name);

            JArray swaggerParameters = new JArray();

            foreach (var key in entitySet.EntityType().Key())
            {
                string format;
                string type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                swaggerParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            foreach (var parameter in operation.Parameters.Skip(1))
            {
                swaggerParameters.Parameter(parameter.Name, operation is IEdmFunction ? "path" : "body",
                    "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            JObject swaggerResponses = new JObject();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name,
                    operation.ReturnType.Definition);
            }

            JObject swaggerOperation = new JObject()
                .Summary("Call operation  " + operation.Name)
                .Description("Call operation  " + operation.Name)
                .Tags(entitySet.Name, operation is IEdmFunction ? "Function" : "Action");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperation.Parameters(swaggerParameters);
            }
            swaggerOperation.Responses(swaggerResponses.DefaultErrorResponse());
            return new JObject()
                {
                    {operation is IEdmFunction ? "get" : "post", swaggerOperation}
                };
        }

        static string GetPathForOperationImport(IEdmOperationImport operationImport)
        {
            string swaggerOperationImportPath = "/" + operationImport.Name + "(";
            if (operationImport.IsFunctionImport())
            {
                foreach (var parameter in operationImport.Operation.Parameters)
                {
                    swaggerOperationImportPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                }
            }
            if (swaggerOperationImportPath.EndsWith(","))
            {
                swaggerOperationImportPath = swaggerOperationImportPath.Substring(0,
                    swaggerOperationImportPath.Length - 1);
            }
            swaggerOperationImportPath += ")";

            return swaggerOperationImportPath;
        }

        static string GetPathForOperationOfEntitySet(IEdmOperation operation, IEdmEntitySet entitySet)
        {
            Debug.Print("GetPathForOperationOfEntitySet-Operation Name: " + operation.Name);

            string swaggerOperationPath = "/" + entitySet.Name + "/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                foreach (var parameter in operation.Parameters.Skip(1))
                {
                    if (parameter.Type.Definition.TypeKind == EdmTypeKind.Primitive &&
                   (parameter.Type.Definition as IEdmPrimitiveType).PrimitiveKind == EdmPrimitiveTypeKind.String)
                    {
                        swaggerOperationPath += parameter.Name + "=" + "'{" + parameter.Name + "}',";
                    }
                    else
                    {
                        swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                    }
                }
            }
            if (swaggerOperationPath.EndsWith(","))
            {
                swaggerOperationPath = swaggerOperationPath.Substring(0,
                    swaggerOperationPath.Length - 1);
            }
            swaggerOperationPath += ")";

            return swaggerOperationPath;
        }

        static string GetPathForOperationOfEntity(IEdmOperation operation, IEdmEntitySet entitySet)
        {
            string swaggerOperationPath = GetPathForEntity(entitySet) + "/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                foreach (var parameter in operation.Parameters.Skip(1))
                {
                    if (parameter.Type.Definition.TypeKind == EdmTypeKind.Primitive &&
                   (parameter.Type.Definition as IEdmPrimitiveType).PrimitiveKind == EdmPrimitiveTypeKind.String)
                    {
                        swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                    }
                    else
                    {
                        swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                    }
                }
            }
            if (swaggerOperationPath.EndsWith(","))
            {
                swaggerOperationPath = swaggerOperationPath.Substring(0,
                    swaggerOperationPath.Length - 1);
            }
            swaggerOperationPath += ")";

            return swaggerOperationPath;
        }

        static JObject CreateSwaggerDefinitionForStructureType(IEdmStructuredType edmType)
        {
            JObject swaggerProperties = new JObject();
            foreach (var property in edmType.StructuralProperties())
            {
                JObject swaggerProperty = new JObject().Description(property.Name);
                SetSwaggerType(swaggerProperty, property.Type.Definition);
                swaggerProperties.Add(property.Name, swaggerProperty);
            }
            return new JObject()
            {
                {"properties", swaggerProperties}
            };
        }

        static void Main(string[] args)
        {
            IEdmModel model;
            IEnumerable<EdmError> errors;

            //EdmxReader.TryParse(XmlReader.Create(metadataURI), out model, out errors);
            CsdlReader.TryParse(XmlReader.Create(metadataURI), out model, out errors);


            JObject swaggerDoc = new JObject()
            {
                {"swagger", "2.0"},
                {"info", new JObject()
                {
                    {"title", "Microsoft Graph 1.0"},
                    {"description", "The swagger for Microsoft Graph 1.0"},
                    {"version", version}
                }},
                {"host", host},
                {"schemes", new JArray("http")},
                {"basePath", basePath},
                {"consumes", new JArray("application/json")},
                {"produces", new JArray("application/json")},
            };

            JObject swaggerPaths = new JObject();
            swaggerDoc.Add("paths", swaggerPaths);
            JObject swaggerDefinitions = new JObject();
            swaggerDoc.Add("definitions", swaggerDefinitions);

            foreach (var entitySet in model.EntityContainer.EntitySets())
            {
                swaggerPaths.Add("/" + entitySet.Name, CreatePathItemObjectForEntitySet(entitySet));
                string pathForEntity = GetPathForEntity(entitySet);
                swaggerPaths.Add(pathForEntity, CreatePathItemObjectForEntity(entitySet));

                Debug.Print(entitySet.Name);
                


                //Let's not forget the navigation property paths....
                foreach (IEdmNavigationProperty navProp in entitySet.EntityType().NavigationProperties())
                {

                    EdmMultiplicity navPropMultiplicity = navProp.TargetMultiplicity();
                    if(navPropMultiplicity == EdmMultiplicity.Many) {
                        //CreatePath
                        swaggerPaths.Add(GetPathForEntityNavigationPropertyEntitySet(pathForEntity, navProp), CreatePathItemObjectForNavigationPropertyMultipleEntitySet(navProp));

                        //It's supported so supporting it here... 
                        Debug.Print("Navigation Property Path (shane): " + GetPathForEntityNavigationPropertyEntitySet(pathForEntity, navProp));
                    }
                    else
                    {
                        swaggerPaths.Add(GetPathForEntityNavigationPropertyEntitySet(pathForEntity, navProp), CreatePathItemObjectForNavigationPropertyZeroOrOne(navProp));
                    }

                    swaggerPaths.Add(GetPathForEntityNavigationPropertyEntityAdd(pathForEntity, navProp), CreatePathItemObjectForNavigationPropertyEntitySetAdd(navProp));
                    swaggerPaths.Add(GetPathForEntityNavigationPropertyEntityRemove(pathForEntity, navProp), CreatePathItemObjectForNavigationPropertyEntitySetRemove(navProp));

                }

            }


            foreach (var operationImport in model.EntityContainer.OperationImports())
            {
                swaggerPaths.Add(GetPathForOperationImport(operationImport), CreateSwaggerPathForOperationImport(operationImport));
            }

            foreach (var type in model.SchemaElements.OfType<IEdmStructuredType>())
            {
                swaggerDefinitions.Add(type.FullTypeName(), CreateSwaggerDefinitionForStructureType(type));
            }

            foreach (var operation in model.SchemaElements.OfType<IEdmOperation>())
            {
                // skip unbound operation
                if (!operation.IsBound)
                {
                    continue;
                }

                var boundParameter = operation.Parameters.First();
                var boundType = boundParameter.Type.Definition;

                // skip operation bound to non entity (or entity collection)
                if (boundType.TypeKind == EdmTypeKind.Entity)
                {
                    IEdmEntityType entityType = boundType as IEdmEntityType;
                    foreach (
                        var entitySet in
                            model.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                    {
                        swaggerPaths.Add(GetPathForOperationOfEntity(operation, entitySet), CreateSwaggerPathForOperationOfEntity(operation, entitySet));
                    }
                }

                else if (boundType.TypeKind == EdmTypeKind.Collection &&
                         (boundType as IEdmCollectionType).ElementType.Definition.TypeKind == EdmTypeKind.Entity)
                {
                    IEdmEntityType entityType = boundType as IEdmEntityType;
                    foreach (
                        var entitySet in
                            model.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                    {
                        swaggerPaths.Add(GetPathForOperationOfEntitySet(operation, entitySet), CreateSwaggerPathForOperationOfEntitySet(operation, entitySet));
                    }
                }
            }


            JObject _ref = JObject.Parse(@" {
                'required': [
                    '@odata.id'
                ],
                'properties': {
                    '@odata.id': {
                        'type': 'string',
                        'description': 'A URL uniquely identifying a specific object'
                    }
                }
            }");

            JObject _propertyValue = JObject.Parse(@" {
                'required': [
                    '@odata.id',
                    'value'
                ],
                'properties': {
                    '@odata.context': {
                        'type': 'string',
                        'description': 'oData context'
                    },
                    'value': {
                        'type': 'object',
                        'description': 'The value of property requested'
                    }
                }
            }");


            swaggerDefinitions.Add("_ref", _ref);
            swaggerDefinitions.Add("_propertyValue", _propertyValue);
            swaggerDefinitions.Add("_Error", new JObject()
             {
                {
                    "properties", new JObject()
                    {
                        {"code", new JObject()
                        {
                            {"type", "string"}
                        }
                        },
                        {"message", new JObject()
                        {
                            {"type", "string"}
                        }
                        },
                        {"error", new JObject()
                        {
                            {"$ref", "#/definitions/_InnerError"}
                        }
                        }
                    }
                }
            });

            
            swaggerDefinitions.Add("_InnerError", new JObject()
            {
                {
                    "properties", new JObject()
                    {
                        {"request-id", new JObject()
                        {
                            {"type", "string"}
                        }
                        },
                        {"date", new JObject()
                        {
                            {"type", "string"}
                        }
                        }
                    }
                }
            });

            File.WriteAllText(outputFile, swaggerDoc.ToString());
        }

        public static void SetSwaggerType(JObject jObject, IEdmType edmType)
        {
            if (edmType.TypeKind == EdmTypeKind.Complex || edmType.TypeKind == EdmTypeKind.Entity)
            {
                jObject.Add("$ref", "#/definitions/" + edmType.FullTypeName());
            }
            else if (edmType.TypeKind == EdmTypeKind.Primitive)
            {
                string format;
                string type = GetPrimitiveTypeAndFormat((IEdmPrimitiveType)edmType, out format);
                jObject.Add("type", type);
                if (format != null)
                {
                    jObject.Add("format", format);
                }
            }
            else if (edmType.TypeKind == EdmTypeKind.Enum)
            {
                jObject.Add("type", "string");
            }
            else if (edmType.TypeKind == EdmTypeKind.Collection)
            {
                IEdmType itemEdmType = ((IEdmCollectionType)edmType).ElementType.Definition;
                JObject nestedItem = new JObject();
                SetSwaggerType(nestedItem, itemEdmType);
                jObject.Add("type", "array");
                jObject.Add("items", nestedItem);
            }
        }

        static string GetPrimitiveTypeAndFormat(IEdmPrimitiveType primtiveType, out string format)
        {
            format = null;
            switch (primtiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.String:
                    return "string";
                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                    format = "int32";
                    return "integer";
                case EdmPrimitiveTypeKind.Int64:
                    format = "int64";
                    return "integer";
                case EdmPrimitiveTypeKind.Boolean:
                    return "boolean";
                case EdmPrimitiveTypeKind.Byte:
                    format = "byte";
                    return "string";
                case EdmPrimitiveTypeKind.Date:
                    format = "date";
                    return "string";
                case EdmPrimitiveTypeKind.DateTimeOffset:
                    format = "date-time";
                    return "string";
                case EdmPrimitiveTypeKind.Double:
                    format = "double";
                    return "number";
                case EdmPrimitiveTypeKind.Single:
                    format = "float";
                    return "number";
                default:
                    return "string";
            }
        }

    }
}
