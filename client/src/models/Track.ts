export default class Track{
    title: string;
    runtime: string;
    constructor(jsonData){
        ({
            title: this.title,
            runtime: this.runtime
        } = jsonData);
    }
}