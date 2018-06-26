#!/bin/sh
FILE=/var/www/web/server/chronojump_server_log.html
echo "<?xml version=\"1.0\" encoding=\"UTF-8\"?><html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"es   \"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" /><title>ChronoJump - measurement, management and statistics of sport short-time tests. </title><style type=\"text/css\">@import url(http://www.chronojump.org/style.css);</style></head><body>" > $FILE

echo "<a2>Chronojump server log</a2><table border='0' cellspacing='4'><tr valign='top'><td><b>ID</b></td><td><b>Uploaded</b></td><td><b>Evaluator</b></td><td><b>Persons</b></td><td><b>Jumps<br>(simple)</b></td><td><b>Jumps<br>(reactive)</b></td></tr>" >> $FILE

sqlite3 -html /root/.local/share/Chronojump/database/chronojump_server.db 'SELECT session.uniqueID, session.uploadedDate, SEvaluator.name, (SELECT COUNT(*) FROM person77,personSession77 WHERE person77.uniqueID=personSession77.personID AND session.uniqueID=personSession77.sessionID),(select count(*) from jump where jump.sessionid=session.uniqueID),(select count(*) from jumpRj where jumpRj.sessionid=session.uniqueID) FROM session, SEvaluator WHERE session.evaluatorID = SEvaluator.uniqueID GROUP BY session.uniqueID ORDER BY session.uniqueID DESC' >> $FILE

echo "</table>" >> $FILE
date  >> $FILE
echo "<br>This table complements <a href=\"http://chronojump.org/server/images/evaluators.png\">Chronojump server evaluators graph</a>" >> $FILE
echo "</body></html>" >> $FILE
