import type {AxiosInstance} from "axios";
import Product from "../models/Product.ts";
import {useFilteredList} from "./useFilteredList.ts";

export interface IProductFilters {
    pages: number;
    currentPage: number;
    items: number;
    search: string;
    type: null | number;
    priceLow: string;
    priceHigh: string;
}

const initialFilters: IProductFilters = {
    pages: 0,
    currentPage: 1,
    items: 20,
    search: "",
    type: null,
    priceLow: "",
    priceHigh: ""
};

export const useProductFilters = (client: AxiosInstance) => {

    const {items: products, filters, setFilters, change, setChange, searchRef} =
        useFilteredList<Product, IProductFilters>(
            client,
            "/Product/GetProductsFiltered",
            initialFilters,
            (f) => ({page: f.currentPage, items: f.items, search: f.search, type: f.type, priceLow: f.priceLow, priceHigh: f.priceHigh}),
            (raw) => new Product(raw)
        );

    return {products, filters, setFilters, change, setChange, searchRef};
}
