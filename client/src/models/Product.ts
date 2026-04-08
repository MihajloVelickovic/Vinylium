export default class Product {
    name: string;
    artist: string;
    imageUrl: string;
    releaseDate: string;
    type: number;
    barcode: string;
    catalogNumber: string;
    inwarehouse: boolean;
    runtime: string;
    price: string;
    tracklist: Array<string>;

    constructor(jsonData: any) {
        ({
            barcode: this.barcode,
            catalogNumber: this.catalogNumber,
            name: this.name,
            artist: this.artist,
            imageUrl: this.imageUrl,
            price: this.price,
            runtime: this.runtime,
            type: this.type,
            releaseDate: this.releaseDate,
            inwarehouse: this.inwarehouse,
            tracklist: this.tracklist
        } = jsonData);

    }

    public static evaluateType = (type: number) => {
        switch (type) {
            case 0:
                return "Vinyl"
            case 1:
                return "Cassette"
            case 2:
                return "CD"
            default:
                return "Unknown"
        }
    }

} 