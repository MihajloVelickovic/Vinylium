export const apiError = (e: any, fallback: string): string => {
    const data = e?.response?.data;

    if (typeof data === "string" && data.trim() !== "")
        return data;

    if (typeof data?.message === "string" && data.message.trim() !== "")
        return data.message;

    return e?.message ?? fallback;
};
