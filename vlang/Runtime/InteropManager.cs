using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VLang.Runtime
{
    public class InteropManager
    {
        private List<Assembly> assemblies = new List<Assembly>();
        private Assembly assembly;
        private List<MethodInfo> methods = new List<MethodInfo>();
        private List<String> namespaces = new List<String>();
        private String netruntime = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        private Type[] types;
        private List<Type> Types = new List<Type>();

        public InteropManager()
        {
            assemblies = new List<Assembly>();
            namespaces = new List<String>();
            methods = new List<MethodInfo>();
            Types = new List<Type>();
            netruntime = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            ImportAllReferencesAssemblies();
        }

        public void ImportAllReferencesAssemblies()
        {
            var refs = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Select<AssemblyName, string>(a => a.Name + ".dll");
            ImportAssembly(Assembly.GetExecutingAssembly());
            foreach (var r in refs) ImportAssembly(r);
        }

        public object CreateInstance(string name, object[] args)
        {
            Type type = GetTypeByName(name, args);

            ConstructorInfo constructor = type.GetConstructor(GetTypesFromObjects(args));

            if (type.IsArray && constructor == null)
            {
                constructor = type.GetConstructor(new Type[1] { typeof(int) });
                object instance = constructor.Invoke(new object[1] { args.Length });

                MethodInfo methods = type.GetMethod("SetValue", new Type[] { typeof(Object), typeof(Int32) });

                Type elementtype = GetTypeByName(name.Replace("[", "").Replace("]", ""), args);

                int ind = 0;
                foreach (object arg in args)
                {
                    if (elementtype == typeof(char) && arg.GetType() == typeof(String) && ((String)arg).Length == 1)
                    {
                        methods.Invoke(instance, new object[2] { ((String)arg)[0], ind });
                    }
                    else
                    {
                        methods.Invoke(instance, new object[2] { arg, ind });
                    }
                    ind++;
                }
                return instance;
            }

            return constructor.Invoke(args);
        }

        public object Extract(object instance, string search)
        {
            object root = instance, current = root;

            current = this.GetTypeByName(search);
            if (current != null) return current;
            if (Types.Where(a => a.Namespace != null && a.Namespace.Split('.').Contains(search)).Count() > 0 && instance == null)
            {
                return new NamespaceInfo(search);
            }
            if (instance is NamespaceInfo)
            {
                return ((NamespaceInfo)instance).FindTypeOrNamespace(search, this);
            }
            if (instance == null) return null;
            Type type = instance is Type ? (Type)instance : instance.GetType();
            BindingFlags bflags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            try
            {
                current = new ReflectionMethod(instance, type.GetMethods(bflags).Where(a => a.Name == search).ToArray());
                return current;
            }
            catch { }
            try { current = type.GetField(search, bflags).GetValue(root); return current; }
            catch { }
            try { current = type.GetProperty(search, bflags).GetValue(root); return current; }
            catch { }
            try { current = type.GetNestedType(search, bflags); return current; }
            catch { }
            try { current = type.GetInterface(search); return current; }
            catch { }
            try { current = type.GetEvent(search, bflags); return current; }
            catch { }
            try { current = type.GetMember(search, bflags).First(); return current; }
            catch { }

            return null;
        }

        public Type GetTypeByName(String name)
        {
            Type type = Types.Find(a => a.FullName == name);
            if (type == null)
            {
                foreach (String nm in namespaces)
                {
                    type = Types.Find(a => a.FullName == nm + "." + name);
                    if (type != null) break;
                }
            }
            if (type == null)
            {
                type = Types.Find(a => a.FullName == name && a.GetConstructor(new Type[] { }) != null);
                if (type == null)
                {
                    foreach (String nm in namespaces)
                    {
                        type = Types.Find(a => a.FullName == nm + "." + name && a.GetConstructor(new Type[] { }) != null);
                        if (type != null) break;
                        type = Types.Find(a => a.FullName == nm + "+" + name && a.GetConstructor(new Type[] { }) != null);
                        if (type != null) break;
                    }
                }
            }
            if (type == null)
            {
                type = GetTypeFromSimpleName(name);
            }
            return type;
        }

        public Type GetTypeByName(String name, object[] args)
        {
            Type type = Types.Find(a => a.FullName == name && a.GetConstructor(GetTypesFromObjects(args)) != null);

            if (type == null)
            {
                foreach (String nm in namespaces)
                {
                    type = Types.Find(a => a.FullName == nm + "." + name && a.GetConstructor(GetTypesFromObjects(args)) != null);
                    if (type != null) break;
                    type = Types.Find(a => a.FullName == nm + "+" + name && a.GetConstructor(GetTypesFromObjects(args)) != null);
                    if (type != null) break;
                }
            }
            if (type == null) type = GetTypeFromSimpleName(name);
            if (type == null) throw new Exception("Type not found: " + name);

            //Console.WriteLine(info.GetParameters()[0].ToString());

            return type;
        }

        // TODO fix because this allows types str[]ing and string[] equal
        public Type GetTypeFromSimpleName(string typeName)
        {
            bool isArray = false, isNullable = false;
            if (typeName.IndexOf("[]") != -1)
            {
                isArray = true;
                typeName = typeName.Remove(typeName.IndexOf("[]"), 2);
            }

            if (typeName.IndexOf("?") != -1)
            {
                isNullable = true;
                typeName = typeName.Remove(typeName.IndexOf("?"), 1);
            }
            typeName = typeName.ToLower();
            string parsedTypeName = typeName;
            switch (typeName)
            {
                case "bool":
                case "boolean":
                    parsedTypeName = "System.Boolean";
                    break;

                case "byte":
                    parsedTypeName = "System.Byte";
                    break;

                case "char":
                    parsedTypeName = "System.Char";
                    break;

                case "datetime":
                    parsedTypeName = "System.DateTime";
                    break;

                case "datetimeoffset":
                    parsedTypeName = "System.DateTimeOffset";
                    break;

                case "decimal":
                    parsedTypeName = "System.Decimal";
                    break;

                case "double":
                    parsedTypeName = "System.Double";
                    break;

                case "float":
                    parsedTypeName = "System.Single";
                    break;

                case "int16":
                case "short":
                    parsedTypeName = "System.Int16";
                    break;

                case "int32":
                case "int":
                    parsedTypeName = "System.Int32";
                    break;

                case "int64":
                case "long":
                    parsedTypeName = "System.Int64";
                    break;

                case "object":
                    parsedTypeName = "System.Object";
                    break;

                case "sbyte":
                    parsedTypeName = "System.SByte";
                    break;

                case "string":
                    parsedTypeName = "System.String";
                    break;

                case "timespan":
                    parsedTypeName = "System.TimeSpan";
                    break;

                case "uint16":
                case "ushort":
                    parsedTypeName = "System.UInt16";
                    break;

                case "uint32":
                case "uint":
                    parsedTypeName = "System.UInt32";
                    break;

                case "uint64":
                case "ulong":
                    parsedTypeName = "System.UInt64";
                    break;
            }

            if (parsedTypeName != null)
            {
                if (isArray)
                    parsedTypeName = parsedTypeName + "[]";
                if (isNullable)
                    parsedTypeName = String.Concat("System.Nullable`1[", parsedTypeName, "]");
            }
            return Type.GetType(parsedTypeName);
        }

        public void ImportAssembly(String name)
        {
            try
            {
                if (!System.IO.File.Exists(System.IO.Path.GetFullPath(name)))
                {
                    if (!System.IO.File.Exists(netruntime + name))
                    {
                        string path = SearchLibrary(name);
                        assembly = Assembly.LoadFile(path);
                    }
                    else
                    {
                        assembly = Assembly.LoadFile(netruntime + name);
                    }
                }
                else
                {
                    try
                    {
                        assembly = Assembly.LoadFile(System.IO.Path.GetFullPath(name));
                    }
                    catch
                    {
                        if (!System.IO.File.Exists(System.IO.Path.GetFullPath(name + ".dll")))
                        {
                            if (!System.IO.File.Exists(netruntime + name + ".dll"))
                            {
                                string path = SearchLibrary(name + ".dll");
                                assembly = Assembly.LoadFile(path);
                            }
                            else
                            {
                                assembly = Assembly.LoadFile(netruntime + name + ".dll");
                            }
                        }
                        else assembly = Assembly.LoadFile(System.IO.Path.GetFullPath(name + ".dll"));
                    }
                }
                Module[] modules = assembly.GetModules();
                foreach (Module module in modules)
                {
                    types = module.GetTypes();
                    foreach (Type type in types) methods.AddRange(type.GetMethods());
                    Types.AddRange(types);
                }
            } // I hate c# style try/catch construction.
            catch { throw new Exception("Error loading reflection module " + name); }
        }

        public void ImportAssembly(Assembly assembly)
        {
            try
            {
                Module[] modules = assembly.GetModules();
                foreach (Module module in modules)
                {
                    types = module.GetTypes();
                    foreach (Type type in types) methods.AddRange(type.GetMethods());
                    Types.AddRange(types);
                }
            } // I hate c# style try/catch construction.
            catch { throw new Exception("Error loading reflection module " + assembly.FullName); }
        }

        public void SetValue(object instance, string search, object value)
        {
            object root = instance, current = root;

            current = this.GetTypeByName(search);
            if (current != null)
            {
                root = null;
            }
            if (instance == null) return;
            Type type = instance.GetType();
            BindingFlags bflags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            try { type.GetField(search, bflags).SetValue(root, value); }
            catch { }
            try { type.GetProperty(search, bflags).SetValue(root, value); }
            catch { }
            try { type.GetEvent(search, bflags).AddEventHandler(root, new Action(delegate { ((ICallable)value).Call(new ExecutionContext()); })); }
            catch { }
        }

        public void UseNamespace(String name)
        {
            namespaces.Add(name);
        }

        private String DoCamelCase(String name)
        {
            return name[0].ToString().ToUpper() + name.Substring(1);
        }

        private Type[] GetTypesFromObjects(object[] args)
        {
            List<Type> types = new List<Type>();
            foreach (object arg in args) types.Add(arg.GetType());
            return types.ToArray();
        }

        private String SearchLibrary(String name, String path = null, int depth = 3)
        {
            if (depth <= 0) return null;
            string dirbase = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            dirbase = System.IO.Directory.GetParent(dirbase).Parent.FullName;
            string[] dirs = System.IO.Directory.GetDirectories(path == null ? dirbase : path);
            String found = null;
            foreach (String dir in dirs)
            {
                if (System.IO.File.Exists(dir + "\\" + name)) return dir + "\\" + name;
                else found = SearchLibrary(name, dir, depth - 1);
            }
            return found;
        }

        public class NamespaceInfo
        {
            public string name;

            public NamespaceInfo(string name)
            {
                this.name = name;
            }

            public object FindTypeOrNamespace(string tp, InteropManager am)
            {
                object obj = am.GetTypeByName(name + '.' + tp);
                if (obj == null) obj = new NamespaceInfo(name + '.' + tp);
                return obj;
            }
        }
    }
}