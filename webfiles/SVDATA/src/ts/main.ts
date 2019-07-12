class Main {
    constructor() {

    };

    init() : void {
        console.log("Hello World");
    };
}

window.onload = () => {
    let main = new Main();
    main.init();
};