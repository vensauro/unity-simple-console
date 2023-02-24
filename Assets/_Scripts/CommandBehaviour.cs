using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

public class CommandBehaviour : MonoBehaviour
{

  // struct CommandMetadata
  // {
  //   MethodInfo info;
  // }


  private static List<string> _consoleItems = new List<string>();

  public static List<string> ConsoleItems => _consoleItems;

  private static Dictionary<string, MethodInfo> _commands = new Dictionary<string, MethodInfo>();
  public static Dictionary<string, MethodInfo> Commands => _commands;


  private void OnEnable()
  {
    _commands = CommandBehaviour._registerCommands(typeof(CommandAttribute));
  }

  public static void AddConsoleItem(string consoleItem)
  {
    _consoleItems.Add(consoleItem);
  }

  [Command]
  void start()
  {
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
    _consoleItems.Add("a");
  }

  public static string camelToSnake(string stringCamel)
  {
    return string.Concat(
      stringCamel
        .Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString().ToLower() : x.ToString().ToLower())
    );
  }

  public static Dictionary<string, MethodInfo> _registerCommands(Type attributeType)
  {
    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

    var methods = assemblies.SelectMany(
        assembly => assembly.GetTypes()
          .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static))
          // .SelectMany(t => t.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static))
          .Where(m => m.GetCustomAttributes(attributeType, false).Length > 0)
          .ToArray()
      ).ToDictionary(method => camelToSnake(method.Name));

    return methods;
  }

  [Command]
  void createCube(float scale = 5)
  {
    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
    sphere.transform.localScale = Vector3.one * scale;
    sphere.transform.position = transform.position;
    sphere.transform.parent = transform.parent;
    sphere.GetComponent<Collider>().enabled = false;
    // sphere.GetComponent<Renderer>().material = material;
    Destroy(sphere, 1f);
  }

  [Command]
  static void log(string value)
  {
    print(value);
  }

  [Command]
  void test()
  {
    print(transform.position);
  }


  [Command]
  private static void help()
  {
    print(String.Join(Environment.NewLine, _commands.Select(command => $"{command.Key}: {command.Value}")));
  }
}
