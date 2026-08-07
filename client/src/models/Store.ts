export default class Store {
    id: number;
    name: string;
    address: string;
    city: string;
    contactNumber: string;
    openingHours: string;
    closingHours: string;

    constructor(jsonData: any) {
        ({
            id: this.id,
            name: this.name,
            address: this.address,
            city: this.city,
            contactNumber: this.contactNumber,
            openingTime: this.openingHours,
            closingTime: this.closingHours,
        } = jsonData);

    }
}