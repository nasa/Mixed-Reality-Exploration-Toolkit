<xsl:stylesheet version="1.0" 	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:xs="http://www.w3.org/2001/XMLSchema"
								exclude-result-prefixes="xs">
<xsl:output method="xml" indent="yes"/>

<xsl:variable name="message" select="/xs:schema/xs:element[last()]/xs:complexType/xs:all"/>
<xsl:variable name="contents" select="$message/xs:element[@name='CONTENT']/xs:complexType/xs:all"/>
<xsl:variable name="header" select="/xs:schema/xs:complexType[@name='HEADER']"/>

<xsl:template match="/xs:schema">

	<xsl:element name="SCHEMA">
		<!-- Building the schema ID -->
		<xsl:attribute name="NAME">
		
			<xsl:value-of select="$header/@name"/>
			
			<xsl:variable name="subtype" select="substring-after(xs:element[last()]/@name,'.')"/>
			<xsl:value-of select="xs:element[last()]/@name"/>
			<xsl:choose>
			<xsl:when test="xs:simpleType[@name=concat($subtype,'-TYPE_type')]/xs:restriction/xs:enumeration/@value">
				<xsl:value-of select="concat('.',xs:simpleType[@name=concat($subtype,'-TYPE_type')])/xs:restriction/xs:enumeration/@value"/>
			</xsl:when>
			<xsl:when test="xs:simpleType[@name=concat($subtype,'-SUBTYPE_type')]/xs:restriction/xs:enumeration/@value">
				<xsl:value-of select="concat('.',xs:simpleType[@name=concat($subtype,'-SUBTYPE_type')])/xs:restriction/xs:enumeration/@value"/>
			</xsl:when>
			</xsl:choose>
			
		</xsl:attribute>
		
		<!-- looping through each child element of HEADER -->
		<!-- these child elements each represent a specific field in the message -->
		<xsl:for-each select="$header/xs:all/xs:element">
			<xsl:element name="FIELD">
		
				<xsl:attribute name="NAME">
					<xsl:value-of select="@name"/>
				</xsl:attribute>

				<xsl:attribute name="MODE">
					<xsl:choose>
					<xsl:when test="@minOccurs=0">
						<xsl:value-of select="'OPTIONAL'"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:value-of select="'REQUIRED'"/>
					</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>

				<xsl:attribute name="FIELD_CLASS">
					<xsl:value-of select="'HEADER'"/>
				</xsl:attribute>

				<!--Transforming the type and value of the field depends-->
				<!--on whether the XSD element is complex or simple-->
				<xsl:choose>

				<!-- element is complex with a named type -->
				<xsl:when test="@type">
					<xsl:call-template name="complexType">
						<xsl:with-param name="type" select="@type"/>
					</xsl:call-template>
				</xsl:when>

				<!-- element is a simple type -->
				<xsl:otherwise>
					<xsl:call-template name="simpleType">
						<xsl:with-param name="type" select="xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@type"/>
						<xsl:with-param name="base" select="xs:complexType/xs:simpleContent/xs:extension/@base"/>
						<xsl:with-param name="name" select="@name"/>
						<xsl:with-param name="pwd" select="$header/xs:all"/>
					</xsl:call-template>
				</xsl:otherwise>

				</xsl:choose>

				<xsl:attribute name="DESCRIPTION">
					<xsl:value-of select="xs:annotation/xs:documentation"/>
				</xsl:attribute>

			</xsl:element>
		</xsl:for-each>

		<!-- looping through each child element of CONTENTS -->
		<!-- these child elements each represent a specific field in the message -->
		<xsl:for-each select="$contents/xs:element">

			<!-- indexed fields in a sequence need to be translated differently -->
			<!-- first determine if the field is part of a sequence -->
			<xsl:choose>
			<!-- It's a sequence -->
			<xsl:when test="xs:complexType/xs:sequence">
				<xsl:call-template name="sequenceType">
					<xsl:with-param name="name" select="@name"/>
					<xsl:with-param name="ref" select="xs:complexType/xs:sequence/xs:element/@ref"/>
					<xsl:with-param name="index" select="'n'"/>
				</xsl:call-template>
			</xsl:when>
			<!-- it's a normal field -->
			<xsl:otherwise>
			<xsl:element name="FIELD">
		
				<xsl:attribute name="NAME">
					<xsl:value-of select="@name"/>
				</xsl:attribute>

				<xsl:attribute name="MODE">
					<xsl:choose>
					<xsl:when test="@minOccurs=0">
						<xsl:value-of select="'OPTIONAL'"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:value-of select="'REQUIRED'"/>
					</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>

				<xsl:attribute name="FIELD_CLASS">
					<xsl:value-of select="'STANDARD'"/>
				</xsl:attribute>

				<!--Transforming the type and value of the field depends-->
				<!--on whether the XSD element is complex or simple-->
				<xsl:choose>

				<!-- element is complex with a named type -->
				<xsl:when test="@type">
					<xsl:call-template name="complexType">
						<xsl:with-param name="type" select="@type"/>
					</xsl:call-template>
				</xsl:when>

				<!-- element is a simple type -->
				<xsl:otherwise>
					<xsl:call-template name="simpleType">
						<xsl:with-param name="type" select="xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@type"/>
						<xsl:with-param name="base" select="xs:complexType/xs:simpleContent/xs:extension/@base"/>
						<xsl:with-param name="name" select="@name"/>
						<xsl:with-param name="pwd" select="$contents"/>
					</xsl:call-template>
				</xsl:otherwise>

				</xsl:choose>

				<xsl:attribute name="DESCRIPTION">
					<xsl:value-of select="xs:annotation/xs:documentation"/>
				</xsl:attribute>

			</xsl:element>

			</xsl:otherwise>

			</xsl:choose>
		</xsl:for-each>
	
	</xsl:element>

</xsl:template>

<xsl:template name="complexType">
	<xsl:param name="type"/>

	<xsl:choose>
	<!--Look for simpleType defined by base attribute for restrictions on values-->
	<!-- first look in Defaults.xsd -->
	<xsl:when test="document('Defaults.xsd')/xs:schema/xs:complexType[@name=$type]">
		<xsl:variable name="extension" select="document('Defaults.xsd')/xs:schema/xs:complexType[@name=$type]/xs:simpleContent/xs:extension"/>
		<xsl:choose>
		<!-- First determine if element extends a basic type or a custom named type  -->
		<xsl:when test="contains($extension/@base, 'xs:')">
			<xsl:attribute name="VALUE">
				<!--element contains basic type with no special restrictions on the value-->
			</xsl:attribute>

			<xsl:attribute name="TYPE">
				<xsl:choose>
				<!-- if type attribute is fixed then simply insert the given type -->
				<xsl:when test="$extension/xs:attribute/@fixed">
					<xsl:value-of select="$extension/xs:attribute/@fixed"/>
				</xsl:when>
				
				<!-- type attribute is named with one or more potential types -->
				<xsl:otherwise>
					<xsl:variable name="type" select="$extension/xs:attribute/@type"/>
					<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$type]/xs:restriction/xs:enumeration">
						<xsl:value-of select="@value"/>
						<xsl:if test="position() != last()">
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:for-each>
				</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
		</xsl:when>
		<!-- MAV TODO: element extends a custom named type? -->
		</xsl:choose>
	</xsl:when>
	<!-- MAV TODO: if type not found in defaults, look in fields? -->
	</xsl:choose>

</xsl:template>

<xsl:template name="simpleType">
	<xsl:param name="base"/>
	<xsl:param name="name"/>
	<xsl:param name="type"/>
	<xsl:param name="pwd"/>

	<xsl:choose>
	<!-- First determine if element extends a basic type or a custom named type  -->
	<xsl:when test="contains($base, 'xs:')">
		<xsl:attribute name="VALUE">
			<!--element contains basic type with no special restrictions on the value-->
		</xsl:attribute>

		<xsl:attribute name="TYPE">
			<xsl:choose>
			<!-- if type attribute is fixed then simply insert the given type -->
			<xsl:when test="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed">
				<xsl:value-of select="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed"/>
			</xsl:when>

			<!-- type attribute is named with one or more potential types -->
			<xsl:otherwise>
				<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$type]/xs:restriction/xs:enumeration">
					<xsl:value-of select="@value"/>
					<xsl:if test="position() != last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
			</xsl:choose>
		</xsl:attribute>
	</xsl:when>

	<!-- element extends a custom named type in the message XSD -->
	<xsl:when test="/xs:schema/xs:simpleType[@name=$base]">
		<xsl:attribute name="VALUE">
		<xsl:for-each select="/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:enumeration">
			<xsl:value-of select="@value"/>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
			<xsl:if test="position() = last() and /xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:minInclusive">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:minInclusive">
			<xsl:value-of select="@value"/>
			<xsl:text>+</xsl:text>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
			<xsl:if test="position() = last() and /xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:maxInclusive">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:maxInclusive">
			<xsl:value-of select="@value"/>
			<xsl:text>-</xsl:text>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		</xsl:attribute>

		<xsl:attribute name="TYPE">
			<xsl:choose>
			<!-- if type attribute is fixed then simply insert the given type -->
			<xsl:when test="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed">
				<xsl:value-of select="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed"/>
			</xsl:when>

			<!-- type attribute is named with one or more potential types -->
			<xsl:otherwise>
				<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$type]/xs:restriction/xs:enumeration">
					<xsl:value-of select="@value"/>
					<xsl:if test="position() != last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
			</xsl:choose>
		</xsl:attribute>
	</xsl:when>
	
	<!-- element extends a custom named type in the fields XSD -->
	<!-- looking for the type in fields XSD -->
	<xsl:when test="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$base]">
		<xsl:attribute name="VALUE">
		<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:enumeration">
			<xsl:value-of select="@value"/>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
			<xsl:if test="position() = last() and document('Fields.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:minInclusive">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:minInclusive">
			<xsl:value-of select="@value"/>
			<xsl:text>+</xsl:text>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
			<xsl:if test="position() = last() and document('Fields.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:maxInclusive">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:maxInclusive">
			<xsl:value-of select="@value"/>
			<xsl:text>-</xsl:text>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		</xsl:attribute>

		<xsl:attribute name="TYPE">
			<xsl:choose>
			<!-- if type attribute is fixed then simply insert the given type -->
			<xsl:when test="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed">
				<xsl:value-of select="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed"/>
			</xsl:when>
			<!-- type attribute is named with one or more potential types -->
			<xsl:otherwise>
				<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$type]/xs:restriction/xs:enumeration">
					<xsl:value-of select="@value"/>
					<xsl:if test="position() != last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
			</xsl:choose>
		</xsl:attribute>
	</xsl:when>

	<!-- element extends a custom named type in the fields XSD -->
	<!-- looking for the type in fields XSD -->
	<xsl:when test="document('Defaults.xsd')/xs:schema/xs:simpleType[@name=$base]">
		<xsl:attribute name="VALUE">
		<xsl:for-each select="document('Defaults.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:enumeration">
			<xsl:value-of select="@value"/>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
			<xsl:if test="position() = last() and document('Defaults.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:minInclusive">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="document('Defaults.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:minInclusive">
			<xsl:value-of select="@value"/>
			<xsl:text>+</xsl:text>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
			<xsl:if test="position() = last() and document('Defaults.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:maxInclusive">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="document('Defaults.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:maxInclusive">
			<xsl:value-of select="@value"/>
			<xsl:text>-</xsl:text>
			<xsl:if test="position() != last()">
				<xsl:text>~~</xsl:text>
			</xsl:if>
			<xsl:if test="position() = last() and document('Defaults.xsd')/xs:schema/xs:simpleType[@name=$base]/xs:restriction/xs:minInclusive">
				<xsl:text>~~</xsl:text>
			</xsl:if>
		</xsl:for-each>
		</xsl:attribute>

		<xsl:attribute name="TYPE">
			<xsl:choose>
			<!-- if type attribute is fixed, then insert the given type -->
			<xsl:when test="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed">
				<xsl:value-of select="$pwd/xs:element[@name=$name]/xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@fixed"/>
			</xsl:when>
			<!-- type attribute is named with one or more potential types -->
			<xsl:otherwise>
				<xsl:for-each select="document('Fields.xsd')/xs:schema/xs:simpleType[@name=$type]/xs:restriction/xs:enumeration">
					<xsl:value-of select="@value"/>
					<xsl:if test="position() != last()">
						<xsl:text>,</xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
			</xsl:choose>
		</xsl:attribute>
	</xsl:when>
	</xsl:choose>

</xsl:template>

<xsl:template name="sequenceType">
	<xsl:param name="name"/>
	<xsl:param name="ref"/>
	<xsl:param name="index"/>

	<!-- add in control field starting array of fields -->
	<xsl:call-template name="arrayStart">
		<xsl:with-param name="name" select="$name"/>
		<xsl:with-param name="index" select="$index"/>
	</xsl:call-template>

	<xsl:for-each select="/xs:schema/xs:element[@name=$ref]/xs:complexType/xs:all/xs:element">
	<xsl:choose>
		<xsl:when test="xs:complexType/xs:sequence">
			<xsl:call-template name="arrayStart">
				<xsl:with-param name="name" select="@name"/>
				<xsl:with-param name="prefix" select="concat($ref,'.',$index,'.')"/>
				<xsl:with-param name="index" select="'m'"/>
			</xsl:call-template>

			<xsl:variable name="nestedRef" select="xs:complexType/xs:sequence/xs:element/@ref"/>
			<xsl:for-each select="/xs:schema/xs:element[@name=$nestedRef]/xs:complexType/xs:all/xs:element">
				<xsl:element name="FIELD">
					<xsl:attribute name="NAME">
						<xsl:value-of select="concat($ref,'.',$index,'.',$nestedRef,'.','m','.',@name)"/>
					</xsl:attribute>

					<xsl:attribute name="MODE">
						<xsl:choose>
						<xsl:when test="@minOccurs=0">
							<xsl:value-of select="'OPTIONAL'"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="'REQUIRED'"/>
						</xsl:otherwise>
						</xsl:choose>
					</xsl:attribute>
					
					<xsl:attribute name="FIELD_CLASS">
						<xsl:value-of select="'STANDARD'"/>
					</xsl:attribute>

					<xsl:call-template name="simpleType">
						<xsl:with-param name="type" select="xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@type"/>
						<xsl:with-param name="base" select="xs:complexType/xs:simpleContent/xs:extension/@base"/>
						<xsl:with-param name="name" select="@name"/>
						<xsl:with-param name="pwd" select="/xs:schema/xs:element[@name=$nestedRef]/xs:complexType/xs:all"/>
					</xsl:call-template>

					<xsl:attribute name="DESCRIPTION">
						<xsl:value-of select="xs:annotation/xs:documentation"/>
					</xsl:attribute>

				</xsl:element>
			</xsl:for-each>

			<xsl:call-template name="arrayEnd"/>
		</xsl:when>
	<xsl:otherwise>
		<xsl:element name="FIELD">
			<xsl:attribute name="NAME">
				<xsl:value-of select="concat($ref,'.',$index,'.',@name)"/>
			</xsl:attribute>

			<xsl:attribute name="MODE">
				<xsl:choose>
				<xsl:when test="@minOccurs=0">
					<xsl:value-of select="'OPTIONAL'"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="'REQUIRED'"/>
				</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>

			<xsl:attribute name="FIELD_CLASS">
				<xsl:value-of select="'STANDARD'"/>
			</xsl:attribute>

			<xsl:call-template name="simpleType">
				<xsl:with-param name="type" select="xs:complexType/xs:simpleContent/xs:extension/xs:attribute/@type"/>
				<xsl:with-param name="base" select="xs:complexType/xs:simpleContent/xs:extension/@base"/>
				<xsl:with-param name="name" select="@name"/>
				<xsl:with-param name="pwd" select="/xs:schema/xs:element[@name=$ref]/xs:complexType/xs:all"/>
			</xsl:call-template>

			<xsl:attribute name="DESCRIPTION">
				<xsl:value-of select="xs:annotation/xs:documentation"/>
			</xsl:attribute>

		</xsl:element>
	</xsl:otherwise>
	</xsl:choose>
	</xsl:for-each>

	<!-- finally, add in control field indicating end of the array -->
	<xsl:call-template name="arrayEnd"/>

</xsl:template>

<xsl:template name="arrayStart">
	<xsl:param name="name"/>
	<xsl:param name="prefix"/>
	<xsl:param name="index"/>

	<xsl:element name="FIELD">
		<xsl:attribute name="NAME">
			<xsl:value-of select="'ARRAY-START'"/>
		</xsl:attribute>
		<xsl:attribute name="MODE">
			<xsl:value-of select="'CONTROL'"/>
		</xsl:attribute>
		<xsl:attribute name="VALUE">
			<xsl:value-of select="$index"/>
		</xsl:attribute>
		<xsl:attribute name="SIZE">
			<xsl:value-of select="concat($prefix,'NUM-OF-',$name)"/>
		</xsl:attribute>
		<xsl:attribute name="DESCRIPTION">
		</xsl:attribute>
	</xsl:element>
</xsl:template>

<xsl:template name="arrayEnd">
	<xsl:element name="FIELD">
		<xsl:attribute name="NAME">
			<xsl:value-of select="'ARRAY-END'"/>
		</xsl:attribute>
		<xsl:attribute name="MODE">
			<xsl:value-of select="'CONTROL'"/>
		</xsl:attribute>
		<xsl:attribute name="DESCRIPTION"/>
	</xsl:element>
</xsl:template>

</xsl:stylesheet>
