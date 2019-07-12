import { AjaxHandler } from "./ajax";

export interface IInitData {
    homedir : string,
    error_path : string
}

export interface ISetPageData {
    url : string,
    new_state : boolean
}

export type ITemplateList = { [index: number]: ITemplate, length: number, push: Function, shift: Function };

export interface ILoadCascadingTemplatesData {
    template_list : ITemplateList, 
    next_template : ITemplate, 
    on_load_data : Object
}

export interface ITemplate {
    template_url : string,
    elelement_id : string,
    required_url? : string,
    insert_into_parent? : boolean,
    has_template? : boolean,
    num_children? : number,
    on_load? : (data : Object) => void
}

export class ContentHandler {
    homedir : string;
    templates : { [index: string]: ITemplate };
    last_page : string[];
    ajax : AjaxHandler;

    constructor() {
        this.homedir = '';
        this.templates = {};
        this.last_page = [];
        this.ajax = new AjaxHandler(false);

        //catch history change events
        window.addEventListener('popstate', () => {
            this.setPage({
                url       : window.location.href,
                new_state : false
            });
        });

        window.addEventListener('click', (event : any) => {
            for (let i = 0; i < event.path.length -4; i++) { //-4: Window, Document, HTML and Body are ignored
                //check for link
                if (event.path[i].href == undefined)
                    break;
    
                //check for target
                var target = event.path[i].target == '' ? '_self' : event.path[i].target;
    
                //check for external links
                var http_link  = event.path[i].href.indexOf('http://') == 0;
                var https_link = event.path[i].href.indexOf('https://') == 0;
                var external = http_link || https_link;
    
                if (external) {
                    window.open(event.path[i].href, target);
                } else {
                    if (target == '_self') {
                        this.setPage({
                            url       : event.path[i].href,
                            new_state : true
                        });
                        event.preventDefault();
                        break;
                    } else {
                        window.open(event.path[i].href, target);
                    }
                }
            }
        });
    };

    private getUrlData = function(url : string) : string[] {
        return url.split('/');
    };

    //loads and inserts multiple templates after one another
    private loadCascadingTemplate(params : ILoadCascadingTemplatesData) : void {
        this.ajax.send({
            url: params.next_template.template_url,
            on_complete: (data) => {
                document.getElementById(params.next_template.elelement_id)!.innerHTML = data;

                if (params.template_list.length > 0) {
                    let next_template_tmp = params.template_list.shift();
                    this.loadCascadingTemplate({
                        template_list : params.template_list,
                        next_template : next_template_tmp,
                        on_load_data  : params.on_load_data
                    });
                } else if (params.next_template.on_load != undefined) {
                    params.next_template.on_load(params.on_load_data);
                }
            }
        });
    };

    init(params : IInitData) : void {
        this.homedir = params.homedir || 'home';

        console.log("Setting up Content Handler ...");

        this.registerTemplate('error404', {
            template_url : params.error_path,
            elelement_id: 'content'
        });
        this.setPage({
            url: window.location.href,
            new_state: true
        });
    };

    setPage(params : ISetPageData) : void {
        //getting relative url: bla.com/abc/def --> abc/def
        let rel_url = params.url.substr( params.url.indexOf(window.location.hostname) + window.location.hostname.length + 1 );

        //remove '/' at the end
        if (rel_url.charAt(rel_url.length -1) == '/')
            rel_url = rel_url.substr(0, rel_url.length -1);

        if (rel_url.length <= 0 || rel_url == 'index.html' || rel_url == 'index.php') { //navigate to default location if no location is set
            params.url += this.homedir;
            rel_url = params.url.substr( params.url.indexOf(window.location.hostname) + window.location.hostname.length + 1 );
        }

        let urlData = this.getUrlData(rel_url);
        let urlDataForCallback = this.getUrlData(rel_url);

        let this_template : ITemplate;
        this_template = this.templates[ urlData[urlData.length -1] ]; //default case

        //check if last value is in templates
        if (!(urlData[urlData.length -1] in this.templates)) {
            for (let offset = 0; offset < urlData.length; offset++) {
                //iterate backwarts through the url arguments
                if ((urlData[urlData.length - offset -1] in this.templates)) { //valid template found
                    if (offset <= this.templates[urlData[urlData.length - offset -1]].num_children!) {
                        for (let i = 0; i < offset; i++) {
                            urlData.pop();
                        }
                        this_template = this.templates[ urlData[urlData.length -1] ];
                    } else { //template child are not valid
                        params.url = '/error404';
                        rel_url = 'error404';
                        urlData = [rel_url];
                        this_template = this.templates[rel_url];
                    }
                }
            }
        }

        //check if requiredUrl is fitting
        let required_rel_url = '/' + rel_url.substr(0, rel_url.indexOf( urlData[urlData.length -1] ));
        if (this_template.required_url != undefined && this_template.required_url != required_rel_url) {
            params.url = '/error404';
            rel_url = 'error404';
            urlData = [rel_url];
            this_template = this.templates[rel_url];
        }

        if (params.new_state == true) //should add entry to browser history
            window.history.pushState("object or string", "title", params.url);

        //CHANGE CONTENT
        let templateListTmp : ITemplateList = [];
        let nextTemplateTmp : ITemplate;
        if (this_template.insert_into_parent) { //insert template into parent one
            for (let i = 0; i < urlData.length; i++)
                templateListTmp.push( this.templates[ urlData[i] ] );

            nextTemplateTmp = templateListTmp.shift();
        } else { //just load the new template and use it as main template
            nextTemplateTmp = this_template;
        }

        let data = { //values for callback
            lastUrlParameter : urlDataForCallback[urlDataForCallback.length -1],
            urlParameter     : urlDataForCallback,
            lastPage         : {
                urlParameter     : this.last_page,
                lastUrlParameter : this.last_page[this.last_page.length -1]
            }
        }

        this.loadCascadingTemplate({
            template_list : templateListTmp,
            next_template : nextTemplateTmp,
            on_load_data  : data
        });

        this.last_page = urlData;
    };

    registerTemplate(url_argument : string, template : ITemplate) : void {
        this.templates[url_argument] = {
            template_url       : template.template_url,
            elelement_id       : template.elelement_id,

            required_url       : template.required_url,
            insert_into_parent : template.insert_into_parent || false,
            has_template       : template.has_template || true,
            num_children       : template.num_children || 0,

            on_load            : template.on_load
        };
    };
}