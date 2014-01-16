PAQUETE libiris
---------------

1.- Paginas man

  Para crear la pagina man de libiris ejecutar:

  $ ./make_man

  Esto crea la pagina de manual a partir de la version en xml

  Para visulizar la pagina ejecutar:

  $ ./make_man view


2.- Para empaquetar para debian

  2.1.- Version en .tgz
  ---------------------

 - Si lo que tienes son las fuentes que vienen con el fichero .tgz, hay que descomprimirlo,
   entrar en el directorio y ejecutar:

  $  dpkg-buildpackage -rfakeroot

  Esto creará el fichero .deb y otros en el directorio superior

  -Para hacer "limpieza" ejecutar:

  $ fakeroot debian/rules clean

  2.2.- Version del SVN
  ---------------------

  -Para empaquetar una version que se encuentra en el SVN hay que optener una versión
   "limpia" (sin los ficheros .svn). Para eso hay que exportar.

  -Ir a un directorio de trabajo:

   $ cd exportar

  -Exportar:

      $ svn export http://svn.iearobotics.com/libIris/libiris-1.2 libiris-1.2

  -Entrar en el directorio libiris-1.2 y seguir lo pasos del punto 2.1
  
  -NOTA PARA DESARROLLADORES: Cuando se hacen pruebas de empaquetamiento es
    un rollo el tener que estar exportando, empaquetando y probando. Por eso,
    para las pruebas se puede directamente invocar el comando
    
  $  dpkg-buildpackage -rfakeroot
  
    y probar el fichero .deb creado. Cuando esté todo depurado, generar 
    nuevamente el paquete .deb pero usando el método de exportar.
    
3.- Crear ejecutable para Windows
---------------------------------

  El autoinstalable para Windows se genera de la siguiente manera:
  
  > python setup.py bdist_wininst

  Y esto genera un .exe que realiza la instalacion.
