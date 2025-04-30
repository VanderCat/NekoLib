using System.Dynamic;
using System.Reflection;
using System.Text;

namespace NekoLib.Archive;

internal static class MiniCli {
    public class MiniNameAttribute(string name) : Attribute {
        public string Name = name;
    }
    public class SkipAttribute : Attribute { }

    public class HelpAttribute(string text) : Attribute {
        public string Text = text;
    }
    public static T Parse<T>(IEnumerable<string> args) where T : new() {
        using var enumearator = args.GetEnumerator();
        var instance = new T();
        while (enumearator.MoveNext()) {
            var n = enumearator.Current;
            FieldInfo? f = null;
            if (n.StartsWith("--")) {
                f = typeof(T).GetField(n[2..], BindingFlags.IgnoreCase);
                if (f is null)
                    continue;
            }
            else if (n.StartsWith("-")) {
                foreach (var field in typeof(T).GetFields()) {
                    var mininame = field.GetCustomAttribute<MiniNameAttribute>()?.Name;
                    if (mininame == n[1..]) {
                        f = field;
                        break;
                    }
                }
                if (f is null)
                    continue;
                //f = f ?? throw new ArgumentException($"Unknown arg {n}");
            }
            else {
                continue;
                throw new ArgumentException($"Unknown arg {n}");
            }
            enumearator.MoveNext();
            if (f.FieldType == typeof(bool)) {
                f.SetValue(instance, true);
                continue;
            }
            if (f.FieldType.IsArray) {
                var strs = enumearator.Current.Split(",");
                var t = f.FieldType.GetElementType() ?? throw new Exception();
                var arr = Array.CreateInstance(t, strs.Length);
                for (var i = 0; i < arr.Length; i++) {
                    Console.WriteLine(arr.Length);
                    arr.SetValue(Convert.ChangeType(enumearator.Current, t), i);
                }
                
                f.SetValue(instance, arr);
                continue;
            }

            f.SetValue(instance, Convert.ChangeType(enumearator.Current, f.FieldType));
        }

        return instance;
    }

    public static string GetHelpFor(Type type, string prepend = "") {
        var a = new StringBuilder();
        var val = Activator.CreateInstance(type);
        foreach (var field in type.GetFields()) {
            if ( field.GetCustomAttribute<SkipAttribute>() is not null)
                continue;
            
            a.Append(prepend);
            a.Append("--");
            a.Append(field.Name);
            var mini = field.GetCustomAttribute<MiniNameAttribute>()?.Name;
            if (mini is not null) {
                a.Append("(-");
                a.Append(mini);
                a.Append(')');
            }
            var help = field.GetCustomAttribute<HelpAttribute>()?.Text;
            a.Append(":\t");
            a.Append(help);
            a.Append("\t[");
            a.Append(field.FieldType.Name);
            a.Append("] Default = ");
            a.Append(field.GetValue(val));
            a.Append('\n');
        }

        return a.ToString();
    }

    public static string GetHelpFor<T>(string prepend = "") => GetHelpFor(typeof(T), prepend);
}