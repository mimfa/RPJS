using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiMFa.RP.CSharp.JS
{
    /// <summary>
    /// A Regular Programming Protocol for JavaScript Codes
    /// </summary>
    public class ARP : ARPBase
    {
        public ARP(Func<string, IEnumerable<object>, object> executer, bool all = false, ARP source = null)
        : base(executer, all, source) { }
        public ARP(string pointer, ARPMode mode = ARPMode.Query, bool all = false, ARP source = null)
        : base(pointer, mode, all, source) { }
        public ARP(string pointer, Func<string, IEnumerable<object>, object> executer, ARPMode mode = ARPMode.Query, bool all = false, ARP source = null)
        : base(pointer, executer, mode, all, source) { }
        public ARP(long x, long y, Func<string, IEnumerable<object>, object> executer, bool all = false, ARP source = null)
        : base(x,y, executer, all, source) { }
        public ARP(ARP pointer, bool? all = null)
        : base(pointer, all) { }
        public ARP(ARP pointer, string script, bool? all = null) 
        : base(pointer, script, all) { }

        public override string ElementPointer()
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
        public override string ElementsPointer()
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

        public override ARPBase PerformPointer(params object[] args)
        {
            var pName = "pointer_" + DateTime.Now.Ticks;
            var val = As(pName).Perform(args);
            return new ARP(pName,Execute, ARPMode.Pure);
        }

        public override ARPBase The(int index = 0)=> All().On("["+ index + "]");
        public override ARPBase First()=> All().On("[0]");
        public override ARPBase Last()=> All().On(".slice(-1).pop()");
        public override ARPBase Reverse() => On(".reverse()");
        public override ARPBase Slice(int index = 0, int? length = null) => On(".slice(" + index + (length == null ? ")" : $", {length})"));

        public override ARPBase On(string nextCode) => Format("{0}{1}", nextCode);

        public override ARPBase Follows(string nextCode) => Format("{0};{1}", nextCode);
        public override ARPBase Follows() => Format("{0};");

        public override ARPBase Prepend(string code) => new ARP(this, code + ToString());
        public override ARPBase Append(string code) => new ARP(this, ToString() + code);

        public override bool Wait(long milisecond = 1000)
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

        public override ARPBase Join(string code) => Format("{0},{1}", code);
        public override ARPBase Join() => Format("{0},");
        public override ARPBase Join(string name,string code) => Format("{0},{1}:{2}", CreateString(name), code);
        public override ARPBase Collect() => Format("{{0}}");
        public override ARPBase Array() => Format("[{0}]");
        public override ARPBase Then(string code) => Format("{0}(()=>{{{1}}})()", code);
        public override ARPBase Then() => Format("(()=>{{{0}}})()");

        public override ARPBase Iterate() => Format("Array.from((function*(){{{0}}})())");
        public override ARPBase Yield(string code) => Format("{0}; yield {1}",code);
        public override ARPBase Yield() => Format(" yield {0}");
        public override ARPBase Return(string code) => Format("{0}; return {1}", code);
        public override ARPBase Return() => Format(" return {0}");

        public override ARPBase If(string conditionCode) => Format("if({1}) ", conditionCode).Then(this);
        public override ARPBase If() => Format("if({0}) ");
        public override ARPBase Else(string conditionCode) => Else().Then(conditionCode);
        public override ARPBase Else() => Format("{0}; else ");
        public override ARPBase Where(string conditionCode) => Format("({1})? ", conditionCode).On(this);
        public override ARPBase Where() => Format("({0})? ");
        public override ARPBase ElseWhere(string code) => ElseWhere().On(code);
        public override ARPBase ElseWhere() => Format("{0} : ");
        public override ARPBase While(string conditionCode) => Format("while({1}) ", conditionCode).Then(this);
        public override ARPBase While() => Format("while({0}) ");
        public override ARPBase ForEach(string elementName,string collectionCode) => Format("for(let {1} of {2}) ", elementName, collectionCode).Then(this);
        public override ARPBase ForEach(string elementName) => Format("for(let {1} of {0}) ", elementName);
        public override ARPBase ForEach() => Format("for(let {1} of {0}) {1}", "element");
        public override ARPBase ForIn(string elementName, string collectionCode) => Format("for(let {1} in {2}) ", elementName, collectionCode).Then(this);
        public override ARPBase ForIn(string elementName) => Format("for(let {1} in {0}) ", elementName);
        public override ARPBase ForIn() => Format("for(let {1} in {0}) {1}", "element");

        public override ARPBase As(string elementName,string code) => Format("(({1})=>{2})({0})", elementName, code);
        public override ARPBase As(string elementName) => Format("{1} = (()=>{{{0}}})()", elementName);
        public override ARPBase Var(string elementName) => Format("var {1} = {0};", elementName);
        public override ARPBase Let(string elementName) => Format("let {1} = {0};", elementName);
        public override ARPBase Const(string elementName) => Format("const {1} = {0};", elementName);
        public override ARPBase Named(string elementName) => Format("{1}:{0}", elementName);

        public override ARPBase And(string code = "true") => Format("({0} && {1})", code);
        public override ARPBase Or(string code = "true") => Format("({0} || {1})", code);

        public override ARPBase Null() => Format("{0} null");
        public override ARPBase Nothing() => Format("{0} (()=>{{}})()");

        public override ARPBase Not(string code) => Format("({0} !== {1})", code);
        public override ARPBase Not() => Format("(!{0})");

        public override ARPBase Is(string code) => Format(" === {1}", code);
        public override ARPBase IsEquals(string code) => Format("({0} === {1})", code);

        public override ARPBase IsVisible() => IsHidden().Not();
        public override ARPBase IsHidden() => As("element", "element === null || element === undefined || element.offsetLeft < 0").Or(GetStyle().As("element","element.visibility === 'hidden' || element.display === 'none'"));
        public override ARPBase IsExists() => As("element", "element !== null && element !== undefined");   
        public override ARPBase IsUndefined() => Is("undefined");
        public override ARPBase IsNull() => Is("null");

        public override ARPBase Count() => On("Array.from({0}).length");

        public override ARPBase SendKeys(string keys) => Scroll().Follows(InvokeKeyboardEvent(keys, "keydown"));
        public override ARPBase SendText(string text) => Scroll().Follows(InvokeKeyboardEvent(Service.ToHotKeys(text), "keydown"));
        public override ARPBase Scroll() => On(".scrollIntoView({ behavior: 'smooth', block: 'end'})");
        public override ARPBase ScrollX(ARPBase pointer) => On(".scrollLeft").Set(pointer.Clone().PositionX());
        public override ARPBase ScrollX(string code) => On(".scrollLeft").Set(code);
        public override ARPBase ScrollX(int x) => On(".scrollLeft").Set(x);
        public override ARPBase ScrollY(ARPBase pointer) => On(".scrollTop").Set(pointer.Clone().PositionY());
        public override ARPBase ScrollY(string code) => On(".scrollTop").Set(code);
        public override ARPBase ScrollY(int y) => On(".scrollTop").Set(y);
        public override ARPBase PositionX() => On(".offsetLeft");
        public override ARPBase PositionY() => On(".offsetTop");
        public override ARPBase Flow() => On(".blur()");
        public override ARPBase Focus() => On(".focus()");
        public override ARPBase Submit() => Scroll().Follows(On(".submit()"));
        public override ARPBase Click() => Scroll().Follows(On(".click()"));// As("element", "element.scrollIntoView(); element.click();");
        public override ARPBase DoubleClick() => InvokeMouseEvent("dblclick");
        public override ARPBase Hover() => InvokeMouseEvent("mouseenter");
        public override ARPBase InvokeMouseEvent(string eventName = "click") => InvokeEvent("MouseEvent", eventName);
        public override ARPBase InvokeKeyboardEvent(string keys, string eventName = "keypress")
        {
            //InitializeJQuery();
            //return Prepend(@"
                //var e = jQuery.Event(`"+ eventName + @"`);
                //e.keyCode = char.charCodeAt(0);
                //$(").Append(").trigger(e);").For("char","`" +keys.Replace("`","\\`")+ "`.split('')");
            return InvokeEvent("keyboardEvent",eventName,"null","char").ForEach("char", CreateString(keys)+ ".split('')");
        }
        public override ARPBase InvokeEvent(string eventName) => InvokeEvent("Event", eventName);
        public override ARPBase InvokeEvent(string eventType, string eventName, params string[] otherArgs)
        {
            return On(".dispatchEvent(evt);").Prepend(string.Join("",
                "var evt  = document.createEvent(`", eventType, "`);",
                "evt.init" + eventType + "(", CreateString(eventName), ", true, true" + (otherArgs.Length>1 ? ", "+string.Join(", ", otherArgs) :"") + ");"));
        }
        public override ARPBase InvokeEvents(string eventType, string eventName, IEnumerable<string[]> otherArgsList)
        {
            var p = On(".dispatchEvent(evt);").Prepend(string.Join("", "var evt  = document.createEvent(", CreateString(eventType), "`);"));
            foreach (var otherArgs in otherArgsList)
                p.On(string.Join("", "evt.init" + eventType + "(", CreateString(eventName), ", true, true" + (otherArgs.Length > 1 ? ", " + string.Join(", ", otherArgs) : "") + ");"));
            return p;
        }


        public override ARPBase NodeName() => On(".nodeName");
        public override ARPBase NodeType() => On(".nodeType");
        public override ARPBase NodeValue() => On(".nodeValue");
        public override ARPBase NextNode() => On(".nextSibling");
        public override ARPBase PreviousNode() => On(".previousSibling");
        public override ARPBase ParentNode() => On(".parentNode");
        public override ARPBase NormalizeNode() => On(".normalize()");
        public override ARPBase CloneNode(bool withChildren = true) => On(".cloneNode(" + (withChildren + "").ToLower() + ")");


        public override ARPBase Replace(ARPBase pointer) => Parent().On(".replaceChild(" + pointer.ToString() + ","+ToString()+")");
        public override ARPBase Remove() => On(".remove()");
        public override ARPBase Closest(string query) => On(".closest(" + CreateString(query) + ")");
        public override ARPBase Matches(string query) => On(".matches(" + CreateString(query) + ")");
        public override ARPBase Next() => On(".nextElementSibling");
        public override ARPBase Previous() => On(".previousElementSibling");
        public override ARPBase Parent() => On(".parentElement");
        public override ARPBase Children() => On(".children");
        public override ARPBase Child(int index) => Children().On("[" + index + "]");

        public override ARPBase this[int index] { get => Get(index); set => Set(index,value); }
        public override ARPBase this[string name] { get => Get(name); set => Set(name, value); }

        public override ARPBase Get() => new ARP(this);
        public override ARPBase Get(int index) => On("[" + index + "]");
        public override ARPBase Get(string name) => On("[" + CreateString(name) + "]");
        public override ARPBase Set(ARPBase pointer) => On(" = " + pointer.ToString());
        public override ARPBase Set(string value) => On(" = " + CreateString(value));
        public override ARPBase Set(object value) => On(" = " + (value is string ? CreateString(value) : value + ""));

        public override ARPBase GetParent() => On(".parentElement");
        public override ARPBase SetParent(ARPBase pointer) => GetParent().Set(pointer);
        public override ARPBase GetChild(int index) => Children().Get(index);
        public override ARPBase SetChild(int index,ARPBase pointer) => GetChild(index).Set(pointer);
        public override ARPBase ReplaceChild(int index, ARPBase pointer) => As("element", "element.replaceChild("+pointer.ToString()+",element.children[" + index + "])");
        public override ARPBase RemoveChild(ARPBase pointer) => On(".removeChild("+ pointer.ToString() + ")");
        public override ARPBase RemoveChild(int index) => As("element", "element.removeChild(element.children[" + index + "])");
        public override ARPBase HasChild() => On(".hasChildNodes()");
        public override ARPBase HasChild(ARPBase pointer) => On(".contains(" + pointer.ToString() + ")");
        public override ARPBase HasChild(int index) => Children().On(".length>"+ index);
        public override ARPBase GetAttribute(string name) => On(".getAttribute("+ CreateString(name) +")");
        public override ARPBase SetAttribute(string name, string value) => On(".setAttribute(" + CreateString(name) +","+ CreateString(value) +")");
        public override ARPBase RemoveAttribute(string name) => On(".removeAttribute(" + CreateString(name) +")");
        public override ARPBase HasAttribute(string attributeName) => On(".hasAttribute(" + CreateString(attributeName) + ")");
        public override ARPBase HasAttribute() => On(".hasAttributes()");
        public override ARPBase GetId() => On(".id");
        public override ARPBase SetId(string value) => GetId().Set(value);
        public override ARPBase GetName() => GetAttribute("name");
        public override ARPBase SetName(string value) => SetAttribute("name", value);
        public override ARPBase GetTitle() => On(".title");
        public override ARPBase SetTitle(string value) => GetTitle().Set(value);
        public override ARPBase GetContent() => On(".textContent");
        public override ARPBase SetContent(string text) => GetContent().Set(text);
        public override ARPBase GetText() => On(".innerText");
        public override ARPBase SetText(string text) => GetText().Set(text);
        public override ARPBase GetValue() => As("elem","elem.value??elem.innerText");
        public override ARPBase SetValue(string value) => As("elem", "{try{elem.value = " + CreateString(value) + ";}catch{elem.innerText = "+ CreateString(value) +";}}");
        public override ARPBase GetInnerHTML() => On(".innerHTML");
        public override ARPBase SetInnerHTML(string html) => GetInnerHTML().Set(html);
        public override ARPBase GetOuterHTML() => On(".outerHTML");
        public override ARPBase SetOuterHTML(string html) => GetOuterHTML().Set(html);
        public override ARPBase GetStyle() => Format("window.getComputedStyle({0})");
        public override ARPBase SetStyle(string style) => On(".style = " + style);
        public override ARPBase GetStyle(string property) => On(".style."+ Service.ToConcatedName(property.ToLower()));
        public override ARPBase SetStyle(string property, object value) => GetStyle(property).Set(value);
        public override ARPBase GetShadowRoot() => On(".shadowRoot");
        public override ARPBase SetShadowRoot(string mode="closed") => Format(".attachShadow({{mode:{1}}})", CreateString(mode));

        public override string CreateString(object value = null) => value == null? "null": string.Join("","`", (value+"").Replace("`","\\`"), "`");

        public override string ToString()
        {
            if (Multiple) return string.Join("",
                "Array.from((function*(elements) { for(let element of elements) yield (()=>",
                    string.IsNullOrWhiteSpace(Script)? "element": Script,
                ")()})(", ElementsPointer(), "))"
            );
            else return string.IsNullOrWhiteSpace(Script) ?ElementPointer() : Script;
        }

        public static implicit operator bool(ARP pointer) => pointer.TryPerform(false);
        public static implicit operator string(ARP pointer) => pointer.TryPerform("");
        public static implicit operator short(ARP pointer) => pointer.TryPerform<short>(0);
        public static implicit operator int(ARP pointer) => pointer.TryPerform(0);
        public static implicit operator long(ARP pointer) => pointer.TryPerform(0l);
        public static implicit operator float(ARP pointer) => pointer.TryPerform(0F);
        public static implicit operator double(ARP pointer) => pointer.TryPerform(0d);
        public static implicit operator decimal(ARP pointer) => pointer.TryPerform(0m);
    }
}
