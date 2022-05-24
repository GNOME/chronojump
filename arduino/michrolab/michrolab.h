typedef void (*functionPointer) (void);

struct menuEntry {
    String title;
    String description;
    functionPointer function;
};
