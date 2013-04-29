chronojumpdir = $(libdir)/chronojump/python/libIris

chronojump_PYTHON =          \
        __init__.py          \
	IntelHex.py          \
	Pic16_Bootloader.py  \
	Pic16_Firmware.py

TAGS_FILES = $(chronojump_PYTHON)

clean-local:
	rm -rf *.pyc *.pyo

EXTRA_DIST = $(chronojump_PYTHON)
DESTDIR = $(expanded_bindir)
