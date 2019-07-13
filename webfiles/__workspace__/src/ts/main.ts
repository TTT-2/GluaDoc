import { ContentHandler } from "./content_handler";

class Main {
    content_handler : ContentHandler;

    constructor() {
        this.content_handler = new ContentHandler();
    };

    init() : void {
        // add templates
        this.content_handler.registerTemplate('home', {
            template_url: '/SVDATA/data/templates/home.html',
            elelement_id: 'code-container'
        });
        this.content_handler.registerTemplate('overview', {
            template_url: '/SVDATA/data/templates/overview.html',
            elelement_id: 'code-container'
        });
        this.content_handler.registerTemplate('code', {
            template_url: '/SVDATA/data/templates/code.html',
            elelement_id: 'code-container'
        });

        // init the content manager 
        this.content_handler.init({
            homedir : 'home',
            error_path: '/SVDATA/data/templates/error404.html'
        });
    };
}

window.onload = () => {
    let main = new Main();
    main.init();
};