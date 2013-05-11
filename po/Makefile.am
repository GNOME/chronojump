
DESTDIR =
EXTRA_DIST =  


CLEANFILES = $(GMO_FILES)

PACKAGE = @PACKAGE@
BUILD_DIR = GMO
INSTALL_DIR = $(DESTDIR)$(prefix)/share/locale

FILES = \
	ar.po\
	ca.po\
	cs.po\
	de.po\
	dz.po\
	el.po\
	en_GB.po\
	es.po\
	fi.po\
	fr.po\
	gl.po\
	nb.po\
	oc.po\
	pt.po\
	pt_BR.po\
	sl.po\
	sv.po\
	vi.po\
	zh_CN.po


GMO_FILES = $(patsubst %.po,$(BUILD_DIR)/%.gmo,$(FILES))
MO_FILES = $(foreach po,$(FILES), $(INSTALL_DIR)/$(basename $(po))/LC_MESSAGES/$(PACKAGE).mo)

all: $(GMO_FILES)

install: $(MO_FILES)

uninstall:
	for lang in $(basename $(FILES)); do \
		rm -f $(INSTALL_DIR)/$$lang/LC_MESSAGES/$(PACKAGE).mo; \
	done

$(GMO_FILES): $(BUILD_DIR)/%.gmo: %.po
	mkdir -p $(BUILD_DIR)
	$(MSGFMT) '$<' -o '$@'
	
$(MO_FILES): $(INSTALL_DIR)/%/LC_MESSAGES/$(PACKAGE).mo : $(BUILD_DIR)/%.gmo
	mkdir -p $(dir $@)
	@INSTALL_DATA@ '$<' '$@'
