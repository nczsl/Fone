
    public class EmmitTagHelper : TagHelper {
        [HtmlAttributeName("config")]
        public Hui config { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
            output.TagName = string.Empty;
            output.TagMode = TagMode.SelfClosing;
            var hdoc = await config.Build();
            Console.WriteLine(hdoc);
            output.Content.SetHtmlContent(hdoc);
            //return Task.CompletedTask;
        }
    }
    public class HuiConfig {
        internal object source;
        public string key;
        public enum SourceType {
            nothing, entity, entities, dic, dics, table, row, meta
        }
        public SourceType stype { get; private set; }
        #region data port
        public void SetEntity<T>(T s) {
            this.source = s;
            this.stype = SourceType.entity;
        }
        public IDictionary<string, object> Dic {
            get {
                var temp = default(IDictionary<string, object>);
                switch (stype) {
                    case SourceType.dic: temp = this.source as IDictionary<string, object>; break;
                    case SourceType.entity: temp = this.source.GetEoo() as IDictionary<string, object>; break;
                }
                return temp;
            }
            set {
                this.source = value;
                this.stype = SourceType.dic;
            }
        }
        public IEnumerable<object> Entities {
            get => source as IEnumerable<object>;
            set {
                this.source = value;
                stype = SourceType.entities;
            }
        }
        public DataTable Table {
            get => source as DataTable;
            set {
                this.source = value;
                stype = SourceType.table;
            }
        }
        public DataRow Row {
            get => source as DataRow;
            set {
                source = value;
                stype = SourceType.row;
            }
        }
        public IEnumerable<IDictionary<string, object>> Dics {
            get => source as IEnumerable<IDictionary<string, object>>;
            set {
                source = value;
                stype = SourceType.dics;
            }
        }
        public IEnumerable<PropertyInfo> MetaData {
            get {
                return source as IEnumerable<PropertyInfo>;
            }
            set {
                source = value;
                stype = SourceType.meta;
            }
        }
        #endregion
        //
        internal XmlFormat hwriter;
        public HuiConfig(Hui owner) {
            hwriter = new XmlFormat("");
            this.owner = owner;
        }
        #region html 常用属性
        public string TagName {
            get => hwriter.Name;
            set => hwriter.Name = value;
        }
        public object TagValue {
            get => hwriter.Value;
            set => hwriter.Value = value;
        }
        internal string TagClass {
            get {
                return hwriter.attributes.Find(i => i.name == "class")?.value?.ToString();
            }
            set {
                if (hwriter.attributes.Exists(i => i.name == "class")) {
                    hwriter.attributes.Find(i => i.name == "class").value = value;
                } else {
                    hwriter.Attribute("class", value);
                }
            }
        }
        internal string TagId {
            get {
                return hwriter.attributes.Find(i => i.name == "id")?.value?.ToString();
            }
            set {
                if (hwriter.attributes.Exists(i => i.name == "id")) {
                    hwriter.attributes.Find(i => i.name == "id").value = value;
                } else {
                    hwriter.Attribute("id", value);
                }
            }
        }
        internal string TagType {
            get {
                return hwriter.attributes.Find(i => i.name == "type")?.value?.ToString();
            }
            set {
                if (hwriter.attributes.Exists(i => i.name == "type")) {
                    hwriter.attributes.Find(i => i.name == "type").value = value;
                } else {
                    hwriter.Attribute("type", value);
                }
            }
        }
        internal string TagHref {
            get {
                return hwriter.attributes.Find(i => i.name == "href")?.value?.ToString();
            }
            set {
                if (hwriter.attributes.Exists(i => i.name == "href")) {
                    hwriter.attributes.Find(i => i.name == "href").value = value;
                } else {
                    hwriter.Attribute("href", value);
                }
            }
        }
        internal string TagAttributeValue {
            get {
                return hwriter.attributes.Find(i => i.name == "value")?.value?.ToString();
            }
            set {
                if (hwriter.attributes.Exists(i => i.name == "value")) {
                    hwriter.attributes.Find(i => i.name == "value").value = value;
                } else {
                    hwriter.Attribute("value", value);
                }
            }
        }
        #endregion
        public object this[string key] {
            get {
                var temp = default(object);
                switch (stype) {
                    case SourceType.entity:
                    case SourceType.dic: temp = Dic[key]; break;
                    case SourceType.row: temp = Row[key]; break;
                    case SourceType.meta:
                        temp = this.MetaData.Single(i => i.Name == key);
                        break;
                }
                return temp;
            }
        }
        public IEnumerable<string> Keys {
            get {
                switch (stype) {
                    case SourceType.entity:
                    case SourceType.dic:
                        foreach (var item in Dic) {
                            yield return item.Key;
                        }
                        break;
                    case SourceType.row:
                        foreach (DataColumn item in Row.Table.Columns) {
                            yield return item.ColumnName;
                        }
                        break;
                    case SourceType.meta:
                        foreach (var item in MetaData) {
                            yield return item.Name;
                        }
                        break;
                }
            }
        }
        public bool CanIndex =>
            stype == SourceType.dic ||
            stype == SourceType.row ||
            stype == SourceType.meta ||
            stype == SourceType.entity;
        public bool CanIterator =>
            stype == SourceType.dics ||
            stype == SourceType.table;
        //stype == SourceType.entity;
        //用于外部测试
        public override string ToString() => hwriter.ToString();
        #region functional port
        public HuiConfig BindingTemplate(string tag, object source, SourceType st) {
            var config = new HuiConfig(owner);
            switch (st) {
                case SourceType.entity: config.SetEntity(source); break;
                case SourceType.entities: config.Entities = source as IEnumerable<object>; break;
                case SourceType.dic: config.Dic = source as IDictionary<string, object>; break;
                case SourceType.dics: config.Dics = source as IEnumerable<IDictionary<string, object>>; break;
                case SourceType.table: config.Table = source as DataTable; break;
                case SourceType.row: config.Row = source as DataRow; break;
                case SourceType.meta: config.MetaData = source as IEnumerable<PropertyInfo>; break;
            }
            config.TagName = tag;
            hwriter.children.Add(config.hwriter);
            owner.emmet.Add(config);
            return config;
        }

        public HuiConfig Binding(string tag, string key) {
            if (CanIndex) {
                return this;
            }
            hwriter.ElementAtChild(tag, this[key]);
            return this;
        }
        public HuiConfig Binding(string tag, string key, string valueformat) {
            if (CanIndex) {
                return this;
            }
            hwriter.ElementAtChild(tag, string.Format(valueformat, this[key]));
            return this;
        }
        public HuiConfig Binding(string tag) {
            if (!CanIndex) {
                return this;
            }
            foreach (var item in Keys) {
                hwriter.ElementAtChild(tag, this[item]);
            }
            return this;
        }
        public HuiConfig Bindings(string gtag, string etag) {
            switch (stype) {
                case SourceType.dics: break;
                case SourceType.table:
                    foreach (DataRow item in Table.Rows) {
                        //var chw = hwriter.ElementAtChild(gtag);
                        foreach (DataColumn it in Table.Columns) {
                            //chw.ElementAtChild(etag, item[it]);
                        }
                    }
                    break;
                case SourceType.entities:
                    foreach (var item in Entities) {
                        var chw = hwriter.ElementAtChild(gtag);
                        var entity = item.GetEoo() as IDictionary<string, object>;
                        foreach (var it in entity) {
                            chw.ElementAtChild(etag, it.Value);
                        }
                    }
                    break;
            }
            return this;
        }
        public HuiConfig Upon() => owner.emmet.Upon(this);
        public Hui owner;
        #endregion
    }

    public class Hui {
        public async Task<string> Build() {
            var r = string.Empty;
            await Task.Run(() => {
                r = emmet.RootData.ToString();
            });
            return r;
        }

        /*
        * Create 一个容器
        * 容器由数据源驱动
        * 容器下面所需要的数据由容器统一提供
        * 容器绑定
        * 枚举方式由外部绑定
        * 通过分派数据源中的一部分或全部的集合数据
        * 来集合绑定
        * 第一步，先只做可索引数据，单实体数据，简单数据，几样，
        */
        internal Chain<HuiConfig> emmet;
        static public HuiConfig CreateTemplate(object source, HuiConfig.SourceType st, string tagname = "div") {
            var instance = new Hui();
            var config = new HuiConfig(instance);
            switch (st) {
                case HuiConfig.SourceType.entity: config.SetEntity(source); break;
                case HuiConfig.SourceType.entities: config.Entities = source as IEnumerable<object>; break;
                case HuiConfig.SourceType.dic: config.Dic = source as IDictionary<string, object>; break;
                case HuiConfig.SourceType.dics: config.Dics = source as IEnumerable<IDictionary<string, object>>; break;
                case HuiConfig.SourceType.table: config.Table = source as DataTable; break;
                case HuiConfig.SourceType.row: config.Row = source as DataRow; break;
                case HuiConfig.SourceType.meta: config.MetaData = source as IEnumerable<PropertyInfo>; break;
            }
            config.TagName = tagname;
            instance.emmet.Add(config);
            return config;
        }
        Hui() {
            emmet = new Chain<HuiConfig>();
        }
        //Fchain.Start<HuiConfig>() handler;
    }