using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiMFa.RP.CSharp
{
    /// <summary>
    /// A Regular Programming Protocol for JavaScript Codes
    /// </summary>
    public class ARPBase : IEnumerable<ARPBase>
    {
        public string Pointer { get; set; } = null;
        public ARPMode Mode { get; set; } = ARPMode.Pure;
        public Func<string, IEnumerable<object>, object> Execute { get; set; } = (s,a) => s;
        public object Evaluate(string code) => Execute(code,new object[] { });
        public bool Multiple { get; set; } = false;
        public ARPBase Source { get; set; } = null;
        public string Script { get; set; } = null;
        public bool AccessToJQuery { get; set; } = false;

        public ARPBase(Func<string, IEnumerable<object>, object> executer, bool all = false, ARPBase source = null)
        {
            Execute = executer;
            Multiple = all;
            Source = source;
            Initialize();
        }
        public ARPBase(string pointer, ARPMode mode = ARPMode.Query, bool all = false, ARPBase source = null)
        {
            Mode = mode;
            Pointer = pointer;
            Multiple = all;
            Source = source;
            Initialize();
        }
        public ARPBase(string pointer, Func<string, IEnumerable<object>, object> executer, ARPMode mode = ARPMode.Query, bool all = false, ARPBase source = null)
        {
            Execute = executer;
            Mode = mode;
            Pointer = pointer;
            Multiple = all;
            Source = source;
            Initialize();
        }
        public ARPBase(long x, long y, Func<string, IEnumerable<object>, object> executer, bool all = false, ARPBase source = null)
        {
            Pointer = string.Join(", ", x, y);
            Mode = ARPMode.Location;
            Execute = executer;
            Multiple = all;
            Source = source;
            Initialize();
        }
        public ARPBase(ARPBase pointer, bool? all = null) : this(pointer, pointer.Script, all)
        {
        }
        public ARPBase(ARPBase pointer, string script, bool? all = null) : this(pointer.Pointer, pointer.Execute, pointer.Mode, all ?? pointer.Multiple, pointer.Source)
        {
            Script = script;
            AccessToJQuery = pointer.AccessToJQuery;
            Initialize();
        }

        public ARPBase Clone() => new ARPBase(this);

        public virtual ARPBase Initialize()
        {
            return this;
        }


        public virtual string ElementPointer()
        {
            var source = Source == null?"document": Source.ToString();
            Multiple = false;
            switch (Mode)
            {
                case ARPMode.Id:
                    return source + ".getElementById(" + CreateString(Pointer) + ")";
                case ARPMode.Name:
                    return source + ".getElementsByName(" + CreateString(Pointer) + ")[0]";
                case ARPMode.Tag:
                    return source + ".getElementsByTagName(" + CreateString(Pointer) + ")[0]";
                case ARPMode.Class:
                    return source + ".getElementsByClassName(" + CreateString(Pointer) + ")[0]";
                case ARPMode.Location:
                    return source + ".elementFromPoint(" + CreateString(Pointer) + ")";
                case ARPMode.Query:
                    return source + ".querySelector(" + CreateString(Pointer) + ")";
                case ARPMode.XPath:
                    return source + ".evaluate(" + CreateString(Pointer) + ", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
                case ARPMode.Pure:
                default:
                    return Pointer;
            }
        }
        public virtual string ElementsPointer()
        {
            var source = Source ==null?"document": Source.ToString();
            Multiple = false;
            switch (Mode)
            {
                case ARPMode.Id:
                    return "["+source + ".getElementById(" + CreateString(Pointer) + ")]";
                case ARPMode.Name:
                    return source + ".getElementsByName(" + CreateString(Pointer) + ")";
                case ARPMode.Tag:
                    return source + ".getElementsByTagName(" + CreateString(Pointer) + ")";
                case ARPMode.Class:
                    return source + ".getElementsByClassName(" + CreateString(Pointer) + ")";
                case ARPMode.Location:
                    return source + ".elementsFromPoint(" + CreateString(Pointer) + ")";
                case ARPMode.Query:
                    return source + ".querySelectorAll(" + CreateString(Pointer) + ")";
                case ARPMode.XPath:
                    return "Array.from((function*(){ let iterator = "+source + ".evaluate(" + CreateString(Pointer) + ", document, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null); let current = iterator.iterateNext(); while(current){ yield current; current = iterator.iterateNext(); }  })())";
                default:
                    return Pointer;
            }
        }

        //public string RootPointerFormat(string format = "{0}", params string[] otherArgs)
        //{
        //    if (Multiple) return string.Join("",
        //        "Array.from((function*(elements) { for(let element of elements) yield (()=>",
        //            string.Format(format, new string[] { string.IsNullOrWhiteSpace(Script) ? "element" : Script }.Concat(otherArgs).ToArray()),
        //        ")()})(", ElementsPointer(), "))"
        //    );
        //    else return string.Format(format, new string[] { string.IsNullOrWhiteSpace(Script) ? ElementPointer() : Script }.Concat(otherArgs).ToArray());
        //}
        //public PointerJS Format(string format = "{0}", params string[] otherArgs) => new PointerJS(this, string.Format(format, new string[] { ToString() }.Concat(otherArgs).ToArray()));

        /// <summary>
        /// Create a JSPointer based on a string format
        /// </summary>
        /// <param name="format">{0} is the current Script</param>
        /// <param name="otherArgs">{0} and next arguments used in 'format'</param>
        /// <returns></returns>
        public ARPBase Format(string format = "{0}", params string[] otherArgs) => new ARPBase(this, string.Format(format, new string[] { ToString() }.Concat(otherArgs).ToArray()));

        public async Task<object> PerformAsync(params object[] args) => await Service.RunTask<object, object>(o => Perform(args));
        public Task PerformTask(params object[] args) => Service.RunTask(() => Perform(args));
        public Thread PerformThread(params object[] args) => Service.Run(() => Perform(args));
        public T TryPerform<T>(T defaultValue = default, params object[] args)
        {
            var o = Perform(args);
            if (o is T)
                return (T)o;
            else return defaultValue;
        }
        public T Perform<T>(params object[] args) => (T)(Perform(args) ?? default(T));public void Perform(Action<object> process, params object[] args)
        {
            var o = Perform(args);
            if (o is IEnumerable<object>)
                Service.Loop((IEnumerable<object>)o, process);
            else process(o);
        }
        public void Perform<TIn>(Action<TIn> process, params object[] args)
        {
            var o = Perform(args);
            if (o is IEnumerable<TIn>)
                Service.Loop((IEnumerable<TIn>)o, process);
            else process((TIn)o);

        }
        public object Perform(Func<object, object> process, params object[] args)
        {
            var o = Perform(args);
            if (o is IEnumerable<object>)
                return Service.Loop((IEnumerable<object>)o, process);
            else return process(o);
        }
        public object Perform<TIn>(Func<TIn, object> process, params object[] args)
        {
            var o = Perform(args);
            if (o is IEnumerable<TIn>)
                return Service.Loop((IEnumerable<TIn>)o, process);
            else return process((TIn)o);
        }
        public virtual object Perform(params object[] args) => Execute(ToString(), args);
        public virtual ARPBase PerformPointer(params object[] args)
        {
            var pName = "pointer_" + DateTime.Now.Ticks;
            var val = As(pName).Perform(args);
            return new ARPBase(pName,Execute, ARPMode.Pure);
        }


        public ARPBase SelectPure(string pointer, bool all = false) => Select(pointer, ARPMode.Pure, all);
        public ARPBase SelectById(string pointer, bool all = false) => Select(pointer, ARPMode.Id, all);
        public ARPBase SelectByTag(string pointer, bool all = false) => Select(pointer, ARPMode.Tag, all);
        public ARPBase SelectByName(string pointer, bool all = false) => Select(pointer, ARPMode.Name, all);
        public ARPBase SelectByClass(string pointer, bool all = false) => Select(pointer, ARPMode.Class, all);
        public ARPBase SelectByXPath(string pointer, bool all = false) => Select(pointer, ARPMode.XPath, all);
        public ARPBase SelectByQuery(string pointer, bool all = false) => Select(pointer, ARPMode.Query, all);
        public ARPBase SelectByLocation(string pointer, bool all = false) => Select(pointer, ARPMode.Location, all);
        public ARPBase SelectByLocation(long x, long y, bool all = false) => Select(x,y, all);
        public ARPBase Select(Func<ARPBase, ARPBase> pointerCreator) => Select(pointerCreator(this));
        public ARPBase Select(string pointer, Func<string, IEnumerable<object>, object> executer, ARPMode pointerMode = ARPMode.Query, bool all = false) => Select(new ARPBase(pointer, executer, pointerMode, all, Source));
        public ARPBase Select(long x, long y, Func<string, IEnumerable<object>, object> executer, bool all = false) => Select(new ARPBase(x,y, executer, all, Source));
        public ARPBase Select(string pointer, ARPMode pointerMode = ARPMode.Query, bool all = false) => Select(new ARPBase(pointer, Execute, pointerMode, all,Source));
        public ARPBase Select(long x, long y, bool all = false) => Select(new ARPBase(x, y, Execute, all, Source));
        public ARPBase Select() => Select(this);
        public virtual ARPBase Select(ARPBase pointer)
        {
            Pointer = pointer.Pointer;
            Mode = pointer.Mode;
            Execute = pointer.Execute;
            Multiple = pointer.Multiple;
            Source = pointer.Source??Source;
            Script = null;
            return this;
        }

        public ARPBase FromPure(string pointer, bool all = false) => From(pointer, ARPMode.Pure, all);
        public ARPBase FromById(string pointer, bool all = false) => From(pointer, ARPMode.Id, all);
        public ARPBase FromByTag(string pointer, bool all = false) => From(pointer, ARPMode.Tag, all);
        public ARPBase FromByName(string pointer, bool all = false) => From(pointer, ARPMode.Name, all);
        public ARPBase FromByClass(string pointer, bool all = false) => From(pointer, ARPMode.Class, all);
        public ARPBase FromByXPath(string pointer, bool all = false) => From(pointer, ARPMode.XPath, all);
        public ARPBase FromByQuery(string pointer, bool all = false) => From(pointer, ARPMode.Query, all);
        public ARPBase FromByLocation(string pointer, bool all = false) => From(pointer, ARPMode.Location, all);
        public ARPBase FromByLocation(long x, long y, bool all = false) => From(x, y, all);
        public ARPBase From(Func<ARPBase, ARPBase> pointerCreator) => From(pointerCreator(this));
        public ARPBase From(string pointer, Func<string, IEnumerable<object>, object> executer, ARPMode pointerMode = ARPMode.Query, bool all = false) => From(new ARPBase(pointer, executer, pointerMode, all));
        public ARPBase From(long x, long y, Func<string, IEnumerable<object>, object> executer, bool all = false) => From(new ARPBase(x, y, executer, all));
        public ARPBase From(string pointer, ARPMode pointerMode = ARPMode.Query, bool all = false) => From(new ARPBase(pointer, Execute, pointerMode, all));
        public ARPBase From(long x, long y, bool all = false) => From(new ARPBase(x, y, Execute, all));
        public ARPBase From() => From(this);
        public virtual ARPBase From(ARPBase pointer)
        {
            Source = pointer;
            return this;
        }


        public virtual ARPBase All()=> new ARPBase(this, true);
        public virtual ARPBase One()=> new ARPBase(this, false);
        public virtual ARPBase The(int index = 0)=> All().On("["+ index + "]");
        public virtual ARPBase First()=> All().On("[0]");
        public virtual ARPBase Last()=> All().On(".slice(-1).pop()");
        public virtual ARPBase Reverse() => On(".reverse()");

        public ARPBase On(ARPBase nextPointer) => On(nextPointer.ToString());
        public virtual ARPBase On(string nextCode) => Format("{0}{1}", nextCode);

        public ARPBase Follows(ARPBase nextPointer) => Follows(nextPointer.ToString());
        public virtual ARPBase Follows(string nextCode) => Format("{0};{1}", nextCode);
        public virtual ARPBase Follows() => Format("{0};");

        public ARPBase Prepend(ARPBase pointer) => Prepend(pointer.ToString());
        public virtual ARPBase Prepend(string code) => new ARPBase(this, code + ToString());
        public ARPBase Append(ARPBase pointer) => Append(pointer.ToString());
        public virtual ARPBase Append(string code) => new ARPBase(this, ToString() + code);

        public virtual bool Wait(long milisecond = 1000)
        {
            //return Not()
            //    .And("(delay -= 1000)>0")
            //    .While()
            //    .Then("new Promise(resolve => setTimeout(resolve, 1000));")
            //    .Prepend("let delay =" + milisecond + ";")
            //    .Return(this)
            //    .TryPerform(false);
            var tick = DateTime.Now.Ticks + milisecond * 10000;
            do
            {
                if (TryPerform(false)) return true;
                Task.Delay(3000);
            } while (tick > DateTime.Now.Ticks);
            return false;
        }

        public ARPBase Join(ARPBase pointer) => Join(pointer.ToString());
        public virtual ARPBase Join(string code) => Format("{0},{1}", code);
        public virtual ARPBase Join() => Format("{0},");
        public ARPBase Join(string name, ARPBase pointer) => Join(name, pointer.ToString());
        public virtual ARPBase Join(string name,string code) => Format("{0},{1}:{2}", CreateString(name), code);
        public virtual ARPBase Collect() => Format("{{0}}");
        public virtual ARPBase Array() => Format("[{0}]");
        public ARPBase Then(ARPBase pointer) => Then(pointer.ToString());
        public virtual ARPBase Then(string code) => Format("{0}(()=>{{{1}}})()", code);
        public virtual ARPBase Then() => Format("(()=>{{{0}}})()");

        /// <summary>
        /// There should be a yield code in the Script
        /// </summary>
        /// <returns></returns>
        public virtual ARPBase Iterate() => Format("Array.from((function*(){{{0}}})())");
        public ARPBase Yield(ARPBase pointer) => Yield(pointer.ToString());
        public virtual ARPBase Yield(string code) => Format("{0}; yield {1}",code);
        public virtual ARPBase Yield() => Format(" yield {0}");
        public ARPBase Return(ARPBase pointer) => Return(pointer.ToString());
        public virtual ARPBase Return(string code) => Format("{0}; return {1}", code);
        public virtual ARPBase Return() => Format(" return {0}");

        public ARPBase If(ARPBase pointer) => If(pointer.ToString());
        public virtual ARPBase If(string conditionCode) => Format("if({1}) ", conditionCode).Then(this);
        public virtual ARPBase If() => Format("if({0}) ");
        public ARPBase Else(ARPBase pointer) => Else(pointer.ToString());
        public virtual ARPBase Else(string conditionCode) => Else().Then(conditionCode);
        public virtual ARPBase Else() => Format("{0}; else ");
        public ARPBase Where(ARPBase pointer) => Where(pointer.ToString());
        public virtual ARPBase Where(string conditionCode) => Format("({1})? ", conditionCode).On(this);
        public virtual ARPBase Where() => Format("({0})? ");
        public ARPBase ElseWhere(ARPBase pointer) => ElseWhere(pointer.ToString());
        public virtual ARPBase ElseWhere(string code) => ElseWhere().On(code);
        public virtual ARPBase ElseWhere() => Format("{0} : ");
        public ARPBase While(ARPBase pointer) => While(pointer.ToString());
        public virtual ARPBase While(string conditionCode) => Format("while({1}) ", conditionCode).Then(this);
        public virtual ARPBase While() => Format("while({0}) ");
        public ARPBase ForEach(string elementName, ARPBase pointer) => ForEach(elementName, pointer.ToString());
        public virtual ARPBase ForEach(string elementName,string collectionCode) => Format("for(let {1} of {2}) ", elementName, collectionCode).Then(this);
        public virtual ARPBase ForEach(string elementName) => Format("for(let {1} of {0}) ", elementName);
        public virtual ARPBase ForEach() => Format("for(let {1} of {0}) {1}", "element");
        public ARPBase ForIn(string elementName, ARPBase pointer) => ForIn(elementName, pointer.ToString());
        public virtual ARPBase ForIn(string elementName, string collectionCode) => Format("for(let {1} in {2}) ", elementName, collectionCode).Then(this);
        public virtual ARPBase ForIn(string elementName) => Format("for(let {1} in {0}) ", elementName);
        public virtual ARPBase ForIn() => Format("for(let {1} in {0}) {1}", "element");

        public ARPBase As(string elementName, ARPBase nextPointer) => As(elementName, nextPointer.ToString());
        public virtual ARPBase As(string elementName,string code) => Format("(({1})=>{2})({0})", elementName, code);
        public virtual ARPBase As(string elementName) => Format("{1} = (()=>{{{0}}})()", elementName);
        public virtual ARPBase Var(string elementName) => Format("var {1} = {0};", elementName);
        public virtual ARPBase Let(string elementName) => Format("let {1} = {0};", elementName);
        public virtual ARPBase Const(string elementName) => Format("const {1} = {0};", elementName);
        public virtual ARPBase Named(string elementName) => Format("{1}:{0}", elementName);

        public ARPBase And(ARPBase pointer) => And(pointer.ToString());
        public virtual ARPBase And(string code = "true") => Format("({0} && {1})", code);
        public ARPBase Or(ARPBase pointer) => Or(pointer.ToString());
        public virtual ARPBase Or(string code = "true") => Format("({0} || {1})", code);

        public virtual ARPBase Null() => Format("{0} null");
        public virtual ARPBase Nothing() => Format("{0} (()=>{{}})()");

        public ARPBase Not(ARPBase pointer) => Not(pointer.ToString());
        public virtual ARPBase Not(string code) => Format("({0} !== {1})", code);
        public virtual ARPBase Not() => Format("(!{0})");

        public ARPBase Is(ARPBase pointer) => Is(pointer.ToString());
        public virtual ARPBase Is(string code) => Format(" === {1}", code);
        public ARPBase IsEquals(ARPBase pointer) => IsEquals(pointer.ToString());
        public virtual ARPBase IsEquals(string code) => Format("({0} === {1})", code);

        public virtual ARPBase IsVisible() => IsHidden().Not();
        public virtual ARPBase IsHidden() => As("element", "element === null || element === undefined || element.offsetLeft < 0").Or(GetStyle().As("element","element.visibility === 'hidden' || element.display === 'none'"));
        public virtual ARPBase IsExists() => As("element", "element !== null && element !== undefined");   
        public virtual ARPBase IsUndefined() => Is("undefined");
        public virtual ARPBase IsNull() => Is("null");

        public virtual ARPBase Count() => On("Array.from({0}).length");

        public virtual ARPBase SendKeys(string keys) => Scroll().Follows(InvokeKeyboardEvent(keys, "keydown"));
        public virtual ARPBase SendText(string text) => Scroll().Follows(InvokeKeyboardEvent(Service.ToHotKeys(text), "keydown"));
        public virtual ARPBase Scroll() => On(".scrollIntoView({ behavior: 'smooth', block: 'end'})"); 
        public virtual ARPBase Flow() => On(".blur()");
        public virtual ARPBase Focus() => On(".focus()");
        public virtual ARPBase Submit() => Scroll().Follows(On(".submit()"));
        public virtual ARPBase Click() => Scroll().Follows(On(".click()"));// As("element", "element.scrollIntoView(); element.click();");
        public virtual ARPBase DoubleClick() => InvokeMouseEvent("dblclick");
        public virtual ARPBase Hover() => InvokeMouseEvent("mouseenter");
        public virtual ARPBase InvokeMouseEvent(string eventName = "click") => InvokeEvent("MouseEvent", eventName);
        public virtual ARPBase InvokeKeyboardEvent(string keys, string eventName = "keypress")
        {
            //InitializeJQuery();
            //return Prepend(@"
                //var e = jQuery.Event(`"+ eventName + @"`);
                //e.keyCode = char.charCodeAt(0);
                //$(").Append(").trigger(e);").For("char","`" +keys.Replace("`","\\`")+ "`.split('')");
            return InvokeEvent("keyboardEvent",eventName,"null","char").ForEach("char", CreateString(keys)+ ".split('')");
        }
        public virtual ARPBase InvokeEvent(string eventName) => InvokeEvent("Event", eventName);
        public virtual ARPBase InvokeEvent(string eventType, string eventName, params string[] otherArgs)
        {
            return On(".dispatchEvent(evt);").Prepend(string.Join("",
                "var evt  = document.createEvent(`", eventType, "`);",
                "evt.init" + eventType + "(", CreateString(eventName), ", true, true" + (otherArgs.Length>1 ? ", "+string.Join(", ", otherArgs) :"") + ");"));
        }
        public virtual ARPBase InvokeEvents(string eventType, string eventName, IEnumerable<string[]> otherArgsList)
        {
            var p = On(".dispatchEvent(evt);").Prepend(string.Join("", "var evt  = document.createEvent(", CreateString(eventType), "`);"));
            foreach (var otherArgs in otherArgsList)
                p.On(string.Join("", "evt.init" + eventType + "(", CreateString(eventName), ", true, true" + (otherArgs.Length > 1 ? ", " + string.Join(", ", otherArgs) : "") + ");"));
            return p;
        }


        public virtual ARPBase NodeName() => On(".nodeName");
        public virtual ARPBase NodeType() => On(".nodeType");
        public virtual ARPBase NodeValue() => On(".nodeValue");
        public virtual ARPBase NextNode() => On(".nextSibling");
        public virtual ARPBase PreviousNode() => On(".previousSibling");
        public virtual ARPBase ParentNode() => On(".parentNode");
        public virtual ARPBase NormalizeNode() => On(".normalize()");
        public virtual ARPBase CloneNode(bool withChildren = true) => On(".cloneNode(" + (withChildren + "").ToLower() + ")");


        public virtual ARPBase Replace(ARPBase pointer) => Parent().On(".replaceChild(" + pointer.ToString() + ","+ToString()+")");
        public virtual ARPBase Remove() => On(".remove()");
        public virtual ARPBase Closest(string query) => On(".closest(" + CreateString(query) + ")");
        public virtual ARPBase Matches(string query) => On(".matches(" + CreateString(query) + ")");
        public virtual ARPBase Next() => On(".nextElementSibling");
        public virtual ARPBase Previous() => On(".previousElementSibling");
        public virtual ARPBase Parent() => On(".parentElement");
        public virtual ARPBase Children() => On(".children");
        public virtual ARPBase Child(int index) => Children().On("[" + index + "]");

        public virtual ARPBase this[int index] { get => Get(index); set => Set(index,value); }
        public virtual ARPBase this[string name] { get => Get(name); set => Set(name, value); }

        public virtual ARPBase Get() => new ARPBase(this);
        public virtual ARPBase Get(int index) => On("[" + index + "]");
        public virtual ARPBase Get(string name) => On("[" + CreateString(name) + "]");
        public virtual ARPBase Set(ARPBase pointer) => On(" = " + pointer.ToString());
        public virtual ARPBase Set(string value) => On(" = " + CreateString(value));
        public virtual ARPBase Set(object value) => On(" = " + (value is string ? CreateString(value) : value + ""));
        public ARPBase Set(int index, ARPBase pointer) => Get(index).Set(pointer);
        public ARPBase Set(string name, ARPBase pointer) => Get(name).Set(pointer);
        public ARPBase Set(int index, string value) => Get(index).Set(value);
        public ARPBase Set(string name, string value) => Get(name).Set(value);
        public ARPBase Set(int index, object value) => Get(index).Set(value);
        public ARPBase Set(string name, object value) => Get(name).Set(value);

        public virtual ARPBase GetParent() => On(".parentElement");
        public virtual ARPBase SetParent(ARPBase pointer) => GetParent().Set(pointer);
        public virtual ARPBase GetChild(int index) => Children().Get(index);
        public virtual ARPBase SetChild(int index,ARPBase pointer) => GetChild(index).Set(pointer);
        public virtual ARPBase ReplaceChild(int index,ARPBase pointer) => As("element", "element.replaceChild("+pointer.ToString()+",element.children[" + index + "])");
        public virtual ARPBase RemoveChild(ARPBase pointer) => On(".removeChild("+ pointer.ToString() + ")");
        public virtual ARPBase RemoveChild(int index) => As("element", "element.removeChild(element.children[" + index + "])");
        public virtual ARPBase HasChild() => On(".hasChildNodes()");
        public virtual ARPBase HasChild(ARPBase pointer) => On(".contains(" + pointer.ToString() + ")");
        public virtual ARPBase HasChild(int index) => Children().On(".length>"+ index);
        public virtual ARPBase GetAttribute(string name) => On(".getAttribute("+ CreateString(name) +")");
        public virtual ARPBase SetAttribute(string name, string value) => On(".setAttribute(" + CreateString(name) +","+ CreateString(value) +")");
        public virtual ARPBase RemoveAttribute(string name) => On(".removeAttribute(" + CreateString(name) +")");
        public virtual ARPBase HasAttribute(string attributeName) => On(".hasAttribute(" + CreateString(attributeName) + ")");
        public virtual ARPBase HasAttribute() => On(".hasAttributes()");
        public virtual ARPBase GetId() => On(".id");
        public virtual ARPBase SetId(string value) => GetId().Set(value);
        public virtual ARPBase GetName() => GetAttribute("name");
        public virtual ARPBase SetName(string value) => SetAttribute("name", value);
        public virtual ARPBase GetTitle() => On(".title");
        public virtual ARPBase SetTitle(string value) => GetTitle().Set(value);
        public virtual ARPBase GetContent() => On(".textContent");
        public virtual ARPBase SetContent(string text) => GetContent().Set(text);
        public virtual ARPBase GetText() => On(".innerText");
        public virtual ARPBase SetText(string text) => GetText().Set(text);
        public virtual ARPBase GetValue() => As("elem","elem.value??elem.innerText");
        public virtual ARPBase SetValue(string value) => As("elem", "{try{elem.value = " + CreateString(value) + ";}catch{elem.innerText = "+ CreateString(value) +";}}");
        public virtual ARPBase GetInnerHTML() => On(".innerHTML");
        public virtual ARPBase SetInnerHTML(string html) => GetInnerHTML().Set(html);
        public virtual ARPBase GetOuterHTML() => On(".outerHTML");
        public virtual ARPBase SetOuterHTML(string html) => GetOuterHTML().Set(html);
        public virtual ARPBase GetStyle() => Format("window.getComputedStyle({0})");
        public virtual ARPBase SetStyle(string style) => On(".style = " + style);
        public virtual ARPBase GetStyle(string property) => On(".style."+ Service.ToConcatedName(property.ToLower()));
        public virtual ARPBase SetStyle(string property, object value) => GetStyle(property).Set(value);
        public virtual ARPBase GetShadowRoot() => On(".shadowRoot");
        public virtual ARPBase SetShadowRoot(string mode="closed") => Format(".attachShadow({{mode:{1}}})", CreateString(mode));


        public virtual string CreateString(object value = null) => value == null? "null": string.Join("","`", (value+"").Replace("`","\\`"), "`");

        public override string ToString()
        {
            if (Multiple) return string.Join("",
                "Array.from((function*(elements) { for(let element of elements) yield (()=>",
                    string.IsNullOrWhiteSpace(Script)? "element": Script,
                ")()})(", ElementsPointer(), "))"
            );
            else return string.IsNullOrWhiteSpace(Script) ?ElementPointer() : Script;
        }

        public IEnumerator<ARPBase> GetEnumerator()
        {
            var pointer = PerformPointer();
            int index = 0;
            while (pointer.Get(index).IsExists().Return().TryPerform(false))
                yield return pointer.Get(index++);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            var pointer = PerformPointer();
            int index = 0;
            while (pointer.Get(index).IsExists().Return().TryPerform(false))
                yield return pointer.Get(index++);
        }

        public static implicit operator bool(ARPBase pointer) => pointer.TryPerform(false);
        public static implicit operator string(ARPBase pointer) => pointer.TryPerform("");
        public static implicit operator short(ARPBase pointer) => pointer.TryPerform<short>(0);
        public static implicit operator int(ARPBase pointer) => pointer.TryPerform(0);
        public static implicit operator long(ARPBase pointer) => pointer.TryPerform(0l);
        public static implicit operator float(ARPBase pointer) => pointer.TryPerform(0F);
        public static implicit operator double(ARPBase pointer) => pointer.TryPerform(0d);
        public static implicit operator decimal(ARPBase pointer) => pointer.TryPerform(0m);
    }
}
