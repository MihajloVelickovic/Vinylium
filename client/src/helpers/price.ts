export const MAX_PRICE = 10_000_000;

export const normalizePrice = (price: string | number | null | undefined): string | null => {
    if (price === null || price === undefined)
        return null;

    const text = String(price).trim().replace(",", ".");
    return text === "" ? null : text;
};

export const validatePrice = (price: string | number | null | undefined): string | null => {
    const normalized = normalizePrice(price);

    if (normalized === null)
        return "Set a price before saving";

    if (!/^-?\d+(\.\d+)?$/.test(normalized))
        return "Price must be a number";

    const value = Number(normalized);

    if (value <= 0)
        return "Price must be greater than zero";

    if (value > MAX_PRICE)
        return `Price cannot be higher than ${MAX_PRICE.toLocaleString("en-US")}`;

    if (!/^\d+(\.\d{1,2})?$/.test(normalized))
        return "Price cannot have more than 2 decimal places";

    return null;
};
