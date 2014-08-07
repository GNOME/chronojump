/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *    (at your option) any later version.^M
 *
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */


using NUnit.Framework;
using System;

namespace EncoderSQLTests {
	[TestFixture]
		public class EncoderSQLTests {
			private EncoderSQL eSQL = null;
			[SetUp]
				public void SetUp()
				{
					eSQL = new EncoderSQL();
				}
			[Test]
				public void ValidateGetDateNoPretty()
				{
					eSQL.Filename = "170-foo bar-2117-2013-10-31_10-12-13.txt";
					string date = eSQL.GetDate(false);
					Assert.AreEqual("2013-10-31_10-12-13", date);
				}
			[Test]
				public void ValidateGetDatePretty()
				{
					eSQL.Filename = "170-foo bar-2117-2013-10-31_10-12-13.txt";
					string date = eSQL.GetDate(true);
					Assert.AreEqual("2013-10-31 10:12:13", date);
				}
			[TearDown]
				public void TearDown()
				{
					eSQL = null;
				}
		} 
}    
