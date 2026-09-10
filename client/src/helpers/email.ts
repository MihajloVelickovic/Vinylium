const EMAIL_PATTERN = /^[\w\-.]+@([\w-]+\.)+[\w-]{2,}$/;

export const validateEmail = (email: string | null | undefined): string | null => {
    const trimmed = (email ?? "").trim();

    if (trimmed === "")
        return "Enter an email address";

    if (!EMAIL_PATTERN.test(trimmed))
        return "Email address format not valid";

    return null;
};
