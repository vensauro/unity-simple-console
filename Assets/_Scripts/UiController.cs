using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UiController : MonoBehaviour
{


  UIDocument? _uiDocument;
  TextField? _textField;
  ListView? _listView;
  ScrollView? _scrollView;
  VisualElement? _resize;
  VisualElement? _maximize;
  VisualElement? _close;

  VisualElement? _root;
  private static List<string> _lastLog = new List<string>();
  int _historicElement = 0;
  // private RectTransform rectTransform;
  private Vector2 currentPointerPosition;
  private Vector2 previousPointerPosition;


  void OnEnable()
  {
    RegisterUi();
  }

  private void RegisterUi()
  {
    var items = CommandBehaviour.ConsoleItems;

    Func<VisualElement> makeItem = () => new Label();
    Action<VisualElement, int> bindItem = (e, i) => (e as Label)!.text = items[i];


    _uiDocument = GetComponent<UIDocument>();
    _root = _uiDocument.rootVisualElement;

    _textField = _root.Q<TextField>("input");

    _listView = _root.Q<ListView>("list");
    _scrollView = _listView.Q<ScrollView>();

    _resize = _root.Q<VisualElement>("resize");
    _maximize = _root.Q<VisualElement>("maximize");
    _close = _root.Q<VisualElement>("close");


    _listView.Q("unity-content-container").focusable = false;
    _listView.makeItem = makeItem;
    _listView.bindItem = bindItem;
    _listView.itemsSource = items;

    _scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(VIGeometryChangedCallback);


    bool mouseDown = false;

    _resize.RegisterCallback<MouseDownEvent>((evt) => mouseDown = true);
    _resize.RegisterCallback<MouseLeaveEvent>((evt) => mouseDown = false);
    _resize.RegisterCallback<MouseUpEvent>((evt) => mouseDown = false);

    var container = _root.Q<VisualElement>("container");

    _resize.RegisterCallback<MouseOverEvent>((evt) =>
    {
      if (mouseDown == false) return;
      container.style.width = new StyleLength(Input.mousePosition.x + 80);
      container.style.height = new StyleLength(Input.mousePosition.y + 20);
    });

    _maximize.RegisterCallback<MouseUpEvent>((evt) =>
    {
      container.style.width = new StyleLength(Length.Percent(100));
      container.style.height = new StyleLength(Length.Percent(40));
    });
    _close.RegisterCallback<MouseUpEvent>((evt) =>
    {
      GetComponent<UIDocument>().enabled = false;
    });


    _textField.RegisterCallback<KeyDownEvent>((evt) => _OnEnter(evt, _textField, _listView));

    Application.logMessageReceived += (string condition, string stackTrace, LogType type) =>
    {
      _lastLog.Clear();
      _lastLog.AddRange(condition.Split(Environment.NewLine));
      _lastLog.ForEach(item => CommandBehaviour.AddConsoleItem(item));
      _listView.RefreshItems();
    };
  }


  void Update()
  {
    if (Input.GetKeyDown(KeyCode.BackQuote))
    {
      _uiDocument!.enabled = true;
      RegisterUi();
    }
  }

  public void VIGeometryChangedCallback(GeometryChangedEvent evt)
  {
    _listView?.ScrollToItem(-1);
    if (CommandBehaviour.ConsoleItems.Count > 0)
      _historicElement = CommandBehaviour.ConsoleItems.Count - 1;
  }

  private void _OnEnter(KeyDownEvent evt, TextField input, ListView list)
  {

    if (evt.keyCode == KeyCode.UpArrow && _historicElement >= 0)
    {
      if (_historicElement < CommandBehaviour.ConsoleItems.Count)
        input.value = CommandBehaviour.ConsoleItems[_historicElement];
      if (_historicElement > 0)
        _historicElement--;
    }

    if (evt.keyCode == KeyCode.DownArrow)
    {
      if (_historicElement < CommandBehaviour.ConsoleItems.Count)
        input.value = CommandBehaviour.ConsoleItems[_historicElement];

      if (CommandBehaviour.ConsoleItems.Count > _historicElement) _historicElement++;
      else input.value = "";
    }

    if (evt.keyCode != KeyCode.Return) return;


    CommandBehaviour.AddConsoleItem(input.value);
    list.RefreshItems();

    _historicElement = CommandBehaviour.ConsoleItems.Count;

    var inputValues = input.value.Split(" ");

    input.value = "";

    if (CommandBehaviour.Commands.TryGetValue(inputValues[0], out var methodInfo))
    {
      ParameterInfo[] parametersInfo = methodInfo.GetParameters();

      var parameters = inputValues.Skip(1).Take(parametersInfo.Length)
          .Select<string, object?>((value, i) => parametersInfo[i].ParameterType switch
          {
            Type t when ReferenceEquals(t, typeof(int)) => int.Parse(value),
            Type t when ReferenceEquals(t, typeof(float)) => float.Parse(value),
            Type t when ReferenceEquals(t, typeof(string)) && value != "null" => value,
            Type t when ReferenceEquals(t, typeof(bool)) => Boolean.Parse(value),
            _ => null
          }).ToArray();

      if (methodInfo.IsStatic)
      {
        methodInfo.Invoke(null, parametersInfo.Length == 0 ? null : parameters);
      }
      else
      {
        Type declaringType = methodInfo.DeclaringType;

        var objectInstance = GameObject.FindFirstObjectByType(declaringType);

        methodInfo.Invoke(objectInstance, parametersInfo.Length == 0 ? null : parameters);
      }
    }
  }
}
