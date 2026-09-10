const PASSWORD_PATTERN = /^[ -~]{8,}$/;

export const validatePassword = (password: string | null | undefined): string | null => {
    const value = password ?? "";

    if (value === "")
        return "Enter a new password";

    if (value.length < 8)
        return "Password must be at least 8 characters";

    if (!PASSWORD_PATTERN.test(value))
        return "Password can only use letters, numbers and symbols";

    return null;
};
