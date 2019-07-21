import { ContentHandler, IOnLoad } from "./content_handler";
import { AjaxHandler } from "./ajax";
import { Dictionary } from "./generic_types";

interface INewElement {
    insert_into : HTMLElement,
    layer : number,
    cssclass : string,
    text : string,
    parent_path : string
};

interface IOpenElement {
    url_elem : string,
    html_elem : HTMLElement
}

interface IIDData {
    level : number,
    path : string[],
    flag? : string
}

var Layer = [
    'type',
    'typename',
    'function'
];

class Main {
    content_handler : ContentHandler;
    ajax : AjaxHandler;
    mainstructure : Dictionary;
    config : Dictionary;

    constructor() {
        this.content_handler = new ContentHandler();
        this.ajax = new AjaxHandler(false);
        this.mainstructure = {};
        this.config = {};
    };

    init() : void {
        //set temporary window title
        document.title = 'loading ...';

        // read config file
        this.ajax.send({
            url         : '/SVDATA/data/config.json',
            type        : 'get',
            on_complete : (response : string) => {
                let response_obj = JSON.parse(response);

                //set window title
                this.config = response_obj;
                document.title = this.config.title;
            }
        });

        // get initial structure
        this.ajax.send({
            url : '/SVDATA/php/request.php',
            type: 'post',
            contents: {
                'action' : 'request',
                'type'   : 'types'
            },
            on_complete : <any>this.receivedConfig.bind(this)
        });

        // add templates
        this.content_handler.registerTemplate({
            url_argument : 'home',
            template_url : '/SVDATA/data/templates/home.html',
            elelement_id : 'code-container'
        });
    };

    receivedConfig(response : string) : void {
        let response_obj = JSON.parse(response);
        this.mainstructure = response_obj;

        // add automatically templates
        for (let template in response_obj) {
            //register topic pages
            this.content_handler.registerTemplate({
                url_argument : template,
                template_url : '/SVDATA/data/templates/overview.html',
                elelement_id : 'code-container',
                num_children : 2,
                on_load      : () => {
                    let url = this.content_handler.getUrl();
                    let elem_data : IIDData = {
                        level : url.length -1,
                        path  : url
                    }
                    let new_pseudo_elem : HTMLElement = document.createElement('li');
                    new_pseudo_elem.id = this.IDfromData(elem_data);

                    this.mainbox({
                        html_elem : new_pseudo_elem,
                        url_elem : url[url.length -1]
                    })
                }
            });
        }

        // init the content manager 
        this.content_handler.init({
            homedir    : 'home',
            error_page : {
                url_argument : 'error404', //has to be error404!
                template_url : '/SVDATA/data/templates/error404.html',
                elelement_id : 'code-container'
            }
        });

        this.initialSetup();

        //this.content_handler.printTemplates();
    };

    // creates the sidebar on initial load
    initialSetup() : void {
        console.log("initial setup...")

        let navlist : HTMLElement = <HTMLElement>document.getElementById('navlist-ul');
        navlist.innerHTML = '';
        for (let key in this.mainstructure) {
            this.addListElement({
                insert_into : navlist,
                layer       : 0,
                cssclass    : Layer[0],
                text        : key,
                parent_path : ''
            });
        }

        //open elements defined by URL
        let url_elem_list = this.content_handler.getUrl()

        //special case: home
        if (url_elem_list.length == 0)
            return;
        if (url_elem_list.length == 1 && url_elem_list[0] == 'home')
            return;
        

        let path_arr : string[] = [];
        for (let i = 0; i < url_elem_list.length; i++) {
            let url_elem = url_elem_list[i];

            path_arr.push(url_elem);

            //element to insert into
            let li_elem_id_data : IIDData = {
                level : i,
                path  : path_arr
            };

            let new_pseudo_elem : HTMLElement = document.createElement('li');
            new_pseudo_elem.id = this.IDfromData(li_elem_id_data);

            this.openLevelAlias[i]({
                url_elem : url_elem,
                html_elem : new_pseudo_elem
            });
        }
    };

    openElement(event : MouseEvent) : void {
        let elem : HTMLElement = <HTMLElement>event.target;

        let id_data = this.dataFromID(elem.id);

        if (id_data.level < 0 || id_data.level >= this.openLevelAlias.length)
            return;

        this.openLevelAlias[id_data.level]({
            url_elem  : elem.innerHTML,
            html_elem : elem
        });
    };

    addListElement(data : INewElement) : void {
        let id_data : IIDData = this.dataFromID(data.insert_into.id);
        
        let id_data_new : IIDData = this.copy(id_data);
        id_data_new.level = data.layer;
        id_data_new.path.push(data.text);
        id_data_new.flag = undefined;
        
        let id_data_new_ul : IIDData = this.copy(id_data);
        id_data_new_ul.level = data.layer;
        id_data_new_ul.path.push(data.text);
        id_data_new_ul.flag = 'ul';

        let new_elem : HTMLElement = document.createElement('li');
        new_elem.classList.add('navlist-element');
        new_elem.classList.add(data.cssclass);
        new_elem.addEventListener('click', this.openElement.bind(this))
        new_elem.innerHTML = data.text;
        new_elem.id = this.IDfromData(id_data_new);

        let new_path : HTMLElement = document.createElement('a');
        let new_url = (data.parent_path == '') ? '/'  + data.text : '/' + data.parent_path + '/' + data.text;
        new_path.setAttribute('href', new_url);
        new_path.appendChild(new_elem);

        let new_ul_elem : HTMLElement = document.createElement('ul');
        new_ul_elem.id = this.IDfromData(id_data_new_ul);
        new_ul_elem.className = 'navlist-' + Layer[data.layer + 1];

        data.insert_into.appendChild(new_path);
        data.insert_into.appendChild(new_ul_elem);
    };

    ///// ALIAS FUNCTIONS //////
    openLevelAlias = [
        this.alias_type.bind(this),
        this.alias_typename.bind(this),
        this.alias_code.bind(this)
    ];

    alias_type (data : IOpenElement) : void {
        // SIDEBAR
        let ul_elem_data = this.dataFromID(data.html_elem.id);
        ul_elem_data.flag = 'ul';

        let ul_elem : HTMLElement = <HTMLElement>document.getElementById(this.IDfromData(ul_elem_data));

        if (ul_elem.innerHTML == '') { // add sublist
            for (let key in this.mainstructure[data.url_elem]) {
                this.addListElement({
                    insert_into : ul_elem,
                    layer       : 1,
                    cssclass    : Layer[1],
                    text        : key,
                    parent_path : data.url_elem
                });
            }
        } else { // remove sublist
            ul_elem.innerHTML = '';
        }

        // MAIN BOX
        this.mainbox(data);
    };
    alias_typename (data : IOpenElement) : void {
        let elem_data = this.dataFromID(data.html_elem.id);

        // data is not available - request from server
        if (this.mainstructure[elem_data.path[0]][elem_data.path[1]].length === 0) {
            this.ajax.send({
                url : '/SVDATA/php/request.php',
                type: 'post',
                contents: {
                    'action' : 'request_elem_list',
                    'args'   : elem_data.path
                },
                passthrough : data,
                on_complete : this.alias_typename_data_fetched.bind(this)
            });
        
        //data is available - direct display
        } else {
            //SIDEBAR
            let ul_elem_data = this.dataFromID(data.html_elem.id);
            ul_elem_data.flag = 'ul';

            let ul_elem : HTMLElement = <HTMLElement>document.getElementById(this.IDfromData(ul_elem_data));

            if (ul_elem.innerHTML == '') { // add sublist
                for (let key in this.mainstructure[ul_elem_data.path[0]][ul_elem_data.path[1]]) {
                    let elem = this.mainstructure[ul_elem_data.path[0]][ul_elem_data.path[1]][key];

                    this.addListElement({
                        insert_into : ul_elem,
                        layer       : 2,
                        cssclass    : elem.param.realm || 'shared',
                        text        : key,
                        parent_path : ul_elem_data.path[0] + '/' + ul_elem_data.path[1]
                    });
                }
            } else { // remove sublist
                ul_elem.innerHTML = '';
            }

            // MAIN BOX
            this.mainbox(data);
        }
    };
    alias_typename_data_fetched (response : string, passthrough : any) : void {
        let data = <IOpenElement>passthrough; // cast type

        let elem_data = this.dataFromID(data.html_elem.id);
        this.mainstructure[elem_data.path[0]][elem_data.path[1]] = JSON.parse(response);

        //call again, data is now available
        this.alias_typename(data);
    };


    alias_code (data : IOpenElement) : void {
        let elem_data = this.dataFromID(data.html_elem.id);

        // this is another not so nice solution to overcome the async problem on page reload
        // it should wait with the request until previous data is available
        if (this.mainstructure[elem_data.path[0]][elem_data.path[1]].length === 0) {
            //data from alias_typename is not yet available
            setTimeout(() => {
                this.alias_code(data);
            }, 25);
            return;
        }

        // data is not available - request from server
        if (this.mainstructure[elem_data.path[0]][elem_data.path[1]][elem_data.path[2]].complete !== true) {
            this.ajax.send({
                url : '/SVDATA/php/request.php',
                type: 'post',
                contents: {
                    'action' : 'request_elem_code',
                    'args'   : elem_data.path
                },
                passthrough : data,
                on_complete : this.alias_code_data_fetched.bind(this)
            });
        
        //data is available - direct display
        } else {
            // MAIN BOX
            this.mainbox(data);
        }
    };
    alias_code_data_fetched (response : string, passthrough : any) : void {
        let data = <IOpenElement>passthrough; // cast type

        let elem_data = this.dataFromID(data.html_elem.id);
        this.mainstructure[elem_data.path[0]][elem_data.path[1]][elem_data.path[2]] = JSON.parse(response);
        this.mainstructure[elem_data.path[0]][elem_data.path[1]][elem_data.path[2]].complete = true;
        
        //call again, data is now available
        this.alias_code(data);
    };

    mainbox (data : IOpenElement) : void {
        let main_elem : HTMLElement = <HTMLElement>document.getElementById('code');
        
        // This is a not so nice solution that waits until all needed data is available
        // since we need to wait for two async data streams to finish.
        // Since it is triggered on json received, we only have to check if the template is loaded
        if (main_elem == undefined) {
            setTimeout(() => {
                this.mainbox(data);
            }, 25);
            return;
        }

        //clear previous contents
        main_elem.innerHTML = '';

        //Draw box here
        let disp_data = this.dataFromID(data.html_elem.id);

        if (disp_data.level == 0) {
            let text : string[][] = [];
            for (let key in this.mainstructure[disp_data.path[0]]) {
                text.push([key]);
            }
            main_elem.appendChild(this.createTableFromData(text, [disp_data.path[0]]));
        }
        if (disp_data.level == 1) {
            let text : string[][] = [];
            for (let key in this.mainstructure[disp_data.path[0]][disp_data.path[1]]) {
                text.push([key]);
            }
            main_elem.appendChild(this.createTableFromData(text, [disp_data.path[1]]));
        }
        if (disp_data.level == 2) {
            main_elem.appendChild(
                this.createCodeDisplay(this.mainstructure[disp_data.path[0]][disp_data.path[1]][disp_data.path[2]])
            );
        }
    }


    // helper
    dataFromID (id : string) : IIDData {
        let delimiter = '###';
        let split = id.split(delimiter);

        // no level yet
        if (split.length <= 1) {
            return {
                level : -1,
                path: []
            };
        } 

        // normal mode
        if (isNaN(parseInt(split[0]))) {
            return {
                flag : split[0],
                level : parseInt(split[1]),
                path: split.slice(2, split.length)
            };
        } else {
            return {
                level : parseInt(split[0]),
                path: split.slice(1, split.length)
            };
        }
    }

    IDfromData (data : IIDData) : string {
        let delimiter = '###';
        let level_str = data.level.toString();
        let arr_cat = [level_str].concat(data.path);

        if (data.flag !== undefined) {
            arr_cat = [data.flag].concat(arr_cat);
        }

        return arr_cat.join(delimiter);
    }

    copy (obj : any) : any {
        return JSON.parse(JSON.stringify(obj))
    }

    createTableFromData (data : string[][], titlebar : string[]) : HTMLElement {
        let table = document.createElement('table');

        for (let title of titlebar) {
            let new_tr = document.createElement('tr');
            let new_th = document.createElement('th');
            new_th.innerHTML = title;
            new_tr.appendChild(new_th);
            table.appendChild(new_tr);
        }

        for (let line of data) {
            // new row
            let new_tr = document.createElement('tr');
            for (let entry of line) {
                let new_td = document.createElement('td');
                new_td.innerHTML = entry;
                new_tr.appendChild(new_td);
            }
            table.appendChild(new_tr);
        }

        return table;
    };

    createCodeDisplay (element : any) : HTMLElement {
        let div = document.createElement('div');
        
        if (element === undefined)
            return div;

        let fun_name = document.createElement('span');
        fun_name.classList.add('code-funcname');
        fun_name.classList.add(element.param.realm || 'shared');
        fun_name.innerHTML = element.name;
        div.appendChild(fun_name);

        let fun_args = document.createElement('span');
        fun_args.classList.add('code-funcargs');
        fun_args.innerHTML = this.parseParams(element.param.param);
        div.appendChild(fun_args);

        let new_line = document.createElement('br');
        div.appendChild(new_line);

        let notes = document.createElement('span');
        notes.classList.add('code-note');
        notes.innerHTML = this.parseNote(element.param.note);
        div.appendChild(notes);
        
        let new_line2 = document.createElement('br');
        div.appendChild(new_line2);

        let source = document.createElement('a');
        source.classList.add('code-source');
        let src_data = this.parseSource(element.path, element.line, this.config.sourcebase);
        source.href = src_data[0];
        source.target = '_blank';
        source.innerHTML = src_data[1];
        div.appendChild(source);

        return div;
    }

    parseParams (params : string[]) : string {
        if (params === undefined)
            return '( )';

        let string = '';

        string += '( ';
        let paramlist : string[] = [];
        for (let param of params) {
            let param_split = param.split(' ');
            if (param_split[0] == 'UNDEFINED')
                param_split[0] = '?';
                paramlist.push(param_split.join(' '));
        }
        string += paramlist.join(', ');
        string += ' )';

        return string
    }

    parseNote (note : string) : string {
        if (note === undefined)
            note = 'None';
        
        return 'Note: ' + note;
    }

    parseSource (url : string, line : number, base : string) : string[] {
        if (url === undefined)
            return ['#', "No Source available."];

        if (line === undefined)
            line = 1;

        return [
            base + '/' + url + '#L' + line.toString(),
            'Source: ' + url + ':' + line.toString()
        ]
    }
}

window.onload = () => {
    let main = new Main();
    main.init();
};